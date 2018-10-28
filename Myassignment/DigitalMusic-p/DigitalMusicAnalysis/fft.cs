using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DigitalMusicAnalysis
{
    class fft
    {
        public Complex[] twiddles;
        public Complex[] fft1(Complex[] x, int wSamp)
        {
            int ii = 0;
            int kk = 0;
            int N = x.Length;

            Complex[] Y = new Complex[N];

            if (N == 1)
            {
                Y[0] = x[0];
            }
            else
            {

                Complex[] E = new Complex[N / 2];
                Complex[] O = new Complex[N / 2];
                Complex[] even = new Complex[N / 2];
                Complex[] odd = new Complex[N / 2];

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

                E = fft1(even, wSamp);
                O = fft1(odd, wSamp);

                for (kk = 0; kk < N; kk++)
                {
                    Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * (wSamp / N)];
                }
            }

            return Y;
        }


    }
}
