using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Primes
{
    public class PrimeHelper
    {
        /// <summary>
        /// Generiert Primzahlen
        /// </summary>
        /// <param name="num">Bis zur Zahl num</param>
        /// <returns></returns>
        public static IEnumerable<int> GetPrimesUntil(int num)
        {
            if (num < 2)
            {
                return new int[0];
            }

            return Enumerable.Range(2, num - 1).AsParallel().Where(x => IsPrime(x));
            //return Enumerable.Range(2, (int)Math.Floor(2.52 * Math.Sqrt(num) / Math.Log(num)))
            //    .Aggregate(Enumerable.Range(2, num - 1).ToList(),
            //        (result, index) =>
            //        {
            //            //if (result.Count <= index)
            //            //{
            //            //    return result;
            //            //}
            //            var bp = result[index - 1];
            //            var sqr = bp * bp;
            //            result.RemoveAll(i => i >= sqr && i % bp == 0);
            //            return result;
            //        }
            //    );
        }

        /// <summary>
        /// Generiert Primzahlen
        /// </summary>
        /// <param name="min">Kleinste Zahl min</param>
        /// <param name="n">n Stk</param>
        /// <returns></returns>
        public static IEnumerable<int> GetPrimes(int min, int n)
        {
            if (min < 2)
            {
                min = 2;
            }

            var r = from i in Enumerable.Range(min, n * n).AsParallel()
                    where Enumerable.Range(1, (int)Math.Sqrt(i)).All(j => j == 1 || i % j != 0)
                    select i;
            return r.Take(n).ToArray();
        }

        public static bool IsPrime(int number)
        {
            if (number == 1) return false;
            if (number == 2 || number == 3 || number == 5) return true;
            if (number % 2 == 0 || number % 3 == 0 || number % 5 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            // You can do less work by observing that at this point, all primes 
            // other than 2 and 3 leave a remainder of either 1 or 5 when divided by 6. 
            // The other possible remainders have been taken care of.
            int i = 6; // start from 6, since others below have been handled.
            while (i <= boundary)
            {
                if (number % (i + 1) == 0 || number % (i + 5) == 0)
                    return false;

                i += 6;
            }

            return true;
        }
    }
}
