using System;


namespace ProbabilisticDistributions {

    /// <summary>
    /// Gerador de números aleatórios implementado para ser utilizado pelo jogo SIMCOW
    /// Autor: Eduardo Carvalho Teixeira em 26/11/2019
    /// Nova versão modificada por Ana Paula Lüdtke Ferreira em 30/12/2019
    /// 
    /// 
    /// </summary>


public abstract class ProbabilisticDistribution {

        protected string name;

        public uint m_w;
        public uint m_z;

        public ProbabilisticDistribution (string name) {
          this.name = name;
          m_w = 0;
          m_z = 0;
        }

        protected void setSeed(uint u, uint v) {
            if (u != 0) 
                m_w = u; 
            if (v != 0)
                m_z = v;
        }

        protected void systemTimeSeed() {
            System.DateTime datetime = System.DateTime.Now;
            long var = datetime.ToFileTime();
            setSeed((uint)(var >> 16), (uint)(var % 4294967296));
        }

        public string getName () { return this.name; }
	public double GenerateValue () { }
    }

    public class UniformDistribution : ProbabilisticDistribution {

        public UniformDistribution () : base ("Uniform") {}

        public double generateValue () {

            systemTimeSeed();

            m_z = 36969 * (m_z & 65535) + (m_z >> 16);
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);


            // 0 <= u < 2^32
            uint u = (m_z << 16) + m_w;
            // Número utilizado é 1/(2^32 + 2).
            // O resultado será entre 0 and 1.
            return (u + 1.0) * 2.328306435454494e-10;
        }
    }

    public class NormalDistribution : ProbabilisticDistribution {
        private double mean;
        private double standard_deviation;
        UniformDistribution u;

        public NormalDistribution (double mean, double sd) : base ("Normal") {
        	this.mean = mean;
                this.standard_deviation = sd;
                this.u = new UniformDistribution ();
        }

        public double generateValue () {

            double u1 = u.generateValue();
            double u2 = u.generateValue();
            double r = Math.Sqrt( -2.0*Math.Log(u1) );
            double theta = 2.0*Math.PI*u2;
            double distn = r*Math.Sin(theta);
            return this.mean + this.standard_deviation * distn;
        }

    }

    public class logNormalDistribution : ProbabilisticDistribution {
        private double logmean;
        private double logstandard_deviation;
        private NormalDistribution n;

       public logNormalDistribution (double logmean, double logsd) : base ("log Normal") {
            this.logmean = logmean;
            this.logstandard_deviation = logsd;
            n = new NormalDistribution(logmean,logsd);
       }

       public double getValue () {
            return Math.Exp(n.getValue);
        }

    }

    public class GammaDistribution : ProbabilisticDistribution {
        private double shape;
        private double scale;
        private NormalDistribution n;
        private UniformDistribution ud;

        public GammaDistribution (double shape, double scale) : base ("Gamma") {
            this.shape = shape;
            this.scale = scale;
            n = new NormalDistribution (0,1);
            ud = new UniformDistribution ();
        }

        public double getValue () {
             double d, c, x, xquadrado, v, u;

            if (this.shape >= 1.0) {
                d = this.shape - 1.0/3.0;
                c = 1.0/Math.Sqrt(9.0*d);
                for (;;) {
                    do
                    {
                        x = n.getValue();
                        v = 1.0 + c*x;
                    }
                    while (v <= 0.0);
                    v = Math.Pow(v, 3);
                    u = ud.getValue();
                    xquadrado = Math.Pow(x, 2);
                    if (u < 1.0 -.0331* Math.Pow(xquadrado, 2) || Math.Log(u) < 0.5* xquadrado + d*(1.0 - v + Math.Log(v)))
                        return scale*d*v;
                }
            }

            else
            {
                double g = new GammaDistribution (shape+1.0, 1.0);
                double w = ud.getValue();
                return this.scale*g.getValue()*Math.Pow(w, 1.0/this.shape);
            }
        }


    }

    public class CauchyDistribution : ProbabilisticDistribution {
        private double median;
        private double scale;
        UniformDistribution u;

        public CauchyDistribution(double median, double scale) : base ("Cauchy") {
            if (scale <= 0) {
                string msg = string.Format("Scale needs to be positive", scale);
                throw new ArgumentException(msg);
            }

            this.median = median;
            this.scale = scale;
            this.u = new UniformDistribution ();
        }

        public double getValue () {

            double p = u.getValue();
            return this.median + this.scale*Math.Tan(Math.PI*(p - 0.5));
        }


    }

    public class WeibullDistribution : ProbabilisticDistribution {

        private double shape;
        private double scale;
        UniformDistribution u;

        public WeibullDistribution (double shape, double scale) : base ("Weibull") {

            if (shape <= 0.0 || scale <= 0.0) {
                string msg = string.Format("Shape and scale need to be positive ", shape, scale);
                throw new ArgumentOutOfRangeException(msg);
            }
            else {
                this.shape = shape;
                this.scale = scale;
                u = new UniformDistribution ();
            }
        }

        public double getValue () {
             return this.scale * Math.Pow(-Math.Log(u.getValue()), 1.0 / this.shape);
        }

    }

}

