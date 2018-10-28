using System;
using System.Numerics;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DigitalMusicAnalysis
{
    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;
        // fft newfft = new fft();
        public int numberofprocessor = System.Environment.ProcessorCount;
        //public int numberofprocessor = 3;
        public float[][] Y;
        public float fftMax = 0;
        public Complex[] X;
        public int N;



        public timefreq(float[] x, int windowSamp)
        {
            //int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];
            //////////////////////////////////////////////////////////
            Parallel.For(0, wSamp, new ParallelOptions { MaxDegreeOfParallelism = numberofprocessor }, ii =>
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            });
            ///////////////////////////////////////////////////////////
            timeFreqData = new float[wSamp/2][];

            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];


            
            for (int kk = 0; kk < nearest; kk++)
            {
                if (kk < x.Length)
                {
                    compX[kk] = x[kk];
                }
                else
                {
                    compX[kk] = Complex.Zero;
                }
            }


            int cols = 2 * nearest /wSamp;

            for (int jj = 0; jj < wSamp / 2; jj++)
            {
                timeFreqData[jj] = new float[cols];
            }

            timeFreqData = stft(compX, wSamp);
	
        }

        float[][] stft(Complex[] x, int wSamp)
        {
            int ii = 0;
            int jj = 0;
            int kk = 0;
            //int ll = 0;
            N = x.Length;
            this.X = x;
            

            //declaration moved to globle level
            //float fftMax = 0;

            //declaration moved to global level.
            Y = new float[wSamp / 2][];

            Parallel.For(0, wSamp / 2, new ParallelOptions { MaxDegreeOfParallelism = numberofprocessor},ll =>
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            });



            var timertimefreqfftcalling = new Stopwatch();
            timertimefreqfftcalling.Start();
            Console.Out.Write("timer started for timefreq fftcalling part  \n");

            ////////////////////////////////////////////////////////////////////////////
            ///


            // Createing an array of thread with the size of number of Processor
            Thread[] threadsarray = new Thread[numberofprocessor];

            for (int thread = 0; thread < numberofprocessor; thread++)
            {

                //creating new thread that calls a parameterized method fftcallingforstft.
                threadsarray[thread] = new Thread(fftcallingforstft);
                threadsarray[thread].Start(thread);
            }
            // Join all the threads.
            for (int thread = 0; thread < numberofprocessor; thread++)
            {
                threadsarray[thread].Join();
                //System.Console.Out.Write("thread  joined {0}\n", thread);
            }


            ////////////////////////////////////////////////////////////////////////////////
            ///
            timertimefreqfftcalling.Stop();
            Console.Out.Write("timefreq fftcalling part  timer ended. Time elapsed: {0} \n", timertimefreqfftcalling.ElapsedMilliseconds);





            for (ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {
                for (kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            }

            return Y;
        }

        public Complex[] fft(Complex[] x)
        {
            int ii = 0;
            int kk = 0;
            int N = x.Length;

            Complex[] Y = new Complex[N];

            // NEED TO MEMSET TO ZERO?

            if (N == 1)
            {
                Y[0] = x[0];
            }
            else{

                Complex[] E = new Complex[N/2];
                Complex[] O = new Complex[N/2];
                Complex[] even = new Complex[N/2];
                Complex[] odd = new Complex[N/2];

                for (ii = 0; ii < N; ii++)
                {

                    if (ii % 2 == 0)
                    {
                        even[ii / 2] = x[ii];
                    }
                    if (ii % 2 == 1)
                    {
                        odd[(ii - 1) / 2] = x[ii];
                    }
                }

                E = fft(even);
                O = fft(odd);

                for (kk = 0; kk < N; kk++)
                {
                    Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * wSamp / N];
                }
            }

           return Y;
        }
         
        public void fftcallingforstft(object data)
        {

            int threadId = (int)data;
            int CHUNK_SIZE = (2 * (int)Math.Floor(N / (double)wSamp) - 1) / numberofprocessor;
            int start = threadId * CHUNK_SIZE;
            int finish = Math.Min(start + CHUNK_SIZE, (2 * (int)Math.Floor(N / (double)wSamp) - 1));
           // System.Console.Out.Write("fftcalling for stft thread {0}, {1} -> {2}\n", threadId, start, finish);
            Complex[] temp = new Complex[wSamp];
            Complex[] tempFFT = new Complex[wSamp];

            for (int ii = start; ii < finish; ii++)
            {

                for (int jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = X[ii * (wSamp / 2) + jj];
                }

                tempFFT = fft(temp);

                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    if (Y[kk][ii] > fftMax)
                    {
                        fftMax = Y[kk][ii];
                    }
                }


            }


        }

        
    }
}
