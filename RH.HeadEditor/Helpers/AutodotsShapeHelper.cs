using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using RH.HeadEditor.Data;

namespace RH.HeadEditor.Helpers
{
    public enum MeshPartType
    {
        None,
        Nose,
        LEye,
        REye,
        Lip,
        Head,
        LEar,
        REar,
        ProfileTop,
        ProfileBottom
    }

    public class AutodotsShapeRect
    {
        public AutodotsShapeRect LinkedShapeRect = null;
        public MeshPartType Type = MeshPartType.None;
        private Vector2[] points;
        public Vector2[] OriginalPoints;
        public int A, B;
        public bool IsLast = false, IsClosed = false;
        public Vector2[] Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
                Coef = new float[points.Length];
                var p0 = points[0];
                var p1 = points[points.Length - 1];
                Coef[0] = 1.0f;
                Coef[points.Length - 1] = 0.0f;
                for (var i = 1; i < points.Length - 1; i++)
                {
                    var pp = points[i];
                    var v = p1 - p0;
                    var w = pp - p0;
                    var c1 = Vector2.Dot(w, v);
                    if (c1 <= 0.0f)
                    {
                        Coef[i] = 1.0f;
                        continue;
                    }
                    var c2 = Vector2.Dot(v, v);
                    if (c2 <= c1)
                    {
                        Coef[i] = 0.0f;
                        continue;
                    }
                    var b = c1 / c2;
                    var pb = p0 + b * v;
                    Coef[i] = 1.0f - (pb - p0).Length / (p1 - p0).Length;
                }
            }
        }
        public float[] Coef;
        public int[] ShapeIndices;
        public void Transform(ref Vector2 a, ref Vector2 b, bool copyToOrigins = true)
        {
            var a0 = points[0];
            var b0 = points[points.Length - 1];
            var da = a - a0;
            var db = b - b0;
            for (var i = 0; i < points.Length; i++)
            {
                var k = Coef[i];
                points[i] = points[i] + (k * da + (1.0f - k) * db);
            }
            if (copyToOrigins)
                points.CopyTo(OriginalPoints, 0);
        }

        public AutodotsShapeRect Initialise(Vector2[] defaultPoints, ref List<Vector2> shapePoints, bool isLast = false, bool isClosed = false)
        {
            IsLast = isLast;
            IsClosed = isClosed;
            const float delta = 0.00001f;
            Points = defaultPoints;
            OriginalPoints = new Vector2[points.Length];
            points.CopyTo(OriginalPoints, 0);
            ShapeIndices = new int[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                var idc = shapePoints.FindIndex(p => Math.Abs(p.X - point.X) < delta && Math.Abs(p.Y - point.Y) < delta);
                if (idc < 0)
                {
                    ShapeIndices[i] = shapePoints.Count;
                    shapePoints.Add(point);
                }
                else
                    ShapeIndices[i] = idc;
            }
            return this;
        }
    }

    public class AutodotsShapeHelper
    {
        #region Dots

        public List<Vector2> ShapeDots = null;

        #region Fem
        public static List<Vector2> ShapeDotsFem = new List<Vector2>
            {
                //face
                new Vector2(-0.981752f, 11.61706f),           //0
                new Vector2(-1.904477f, 11.47961f),           //1
                new Vector2(-3.603256f, 10.83925f),           //2
                new Vector2(-4.454773f, 10.39127f),           //3
                new Vector2(-5.147111f, 9.866611f),           //4
                new Vector2(-6.176281f, 8.701387f),           //5
                new Vector2(-6.624222f, 8.100462f),           //6
                new Vector2(-7.270767f, 6.688686f),           //7
                new Vector2(-7.531153f, 5.701538f),           //8
                new Vector2(-7.776262f, 3.443955f),           //9
                new Vector2(-7.723608f, 2.133899f),           //10
                new Vector2(-7.587543f, 0.9441374f),            //11
                new Vector2(-7.331595f, -0.5710145f),           //12
                new Vector2(-7.276883f, -1.553266f),            //13
                new Vector2(-7.147934f, -2.587496f),            //14
                new Vector2(-7.018986f, -3.485907f),            //15
                new Vector2(-6.770206f, -5.034902f),           //16
                new Vector2(-6.475906f, -5.986256f),            //17
                new Vector2(-6.23605f, -6.65834f),           //18
                new Vector2(-5.901694f, -7.340099f),            //19
                new Vector2(-4.835572f, -8.801191f),            //20
                new Vector2(-3.501713f, -9.884702f),            //21
                new Vector2(-2.772749f, -10.25996f),            //22
                new Vector2(-1.847578f, -10.63449f),            //23
                new Vector2(-0.3269181f, -10.96561f),           //24
                new Vector2(0.01445205f, -5.487587f),           //25 //lip
                new Vector2(-0.4343459f, -5.255089f),           //26
                new Vector2(-1.591457f, -5.691542f),            //27
                new Vector2(-2.066361f, -5.963872f),            //28
                new Vector2(-2.175298f, -6.85472f),           //29
                new Vector2(-0.7648464f, -7.476824f),           //30
                new Vector2(-1.275943f, -2.687708f),           //31 //nose
                new Vector2(-1.697595f, -3.250233f),           //32
                new Vector2(-0.5791314f, -4.276031f),           //33
                new Vector2(-3.772266f, 0.4996974f),           //34 //eyes
                new Vector2(-4.17333f, 0.3388649f),           //35
                new Vector2(-4.151917f, -0.0610379f),           //36
                new Vector2(-3.686683f, -0.218465f),           //37
                new Vector2(-2.80087f, -0.2382792f),           //38
                new Vector2(-2.377171f, -0.1572783f),           //39
                new Vector2(-2.319042f, 0.3357835f),           //40
                new Vector2(-2.728965f, 0.5545042f),           //41
                new Vector2(-9.18974f, 0.5390474f),           //42 //ear
                new Vector2(-8.935152f, -2.162611f),           //43
                new Vector2(-1.636339f, -6.415571f),            //44 //lip center
                new Vector2(-0.9124852f, -6.303484f),           //45
            };
        #endregion

        #region Male
        public static List<Vector2> ShapeDotsMale = new List<Vector2>
            {
                //face
                new Vector2(-0.9789042f, 12.05736f),               //0
                new Vector2(-1.94296f, 11.87606f),                 //1
                new Vector2(-3.564106f, 11.30436f),              //2
                new Vector2(-4.291405f, 10.89455f),              //3
                new Vector2(-4.910184f, 10.41083f),              //4
                new Vector2(-6.00957f, 9.389835f),              //5
                new Vector2(-6.460105f, 8.731473f),                 //6
                new Vector2(-7.128243f, 7.265686f),             //7
                new Vector2(-7.435603f, 6.305461f),              //8
                new Vector2(-7.655846f, 3.999831f),              //9
                new Vector2(-7.675285f, 2.661895f),              //10
                new Vector2(-7.560375f, 1.643476f),             //11
                new Vector2(-7.219309f, -0.1005855f),            //12
                new Vector2(-7.164797f, -1.103741f),             //13
                new Vector2(-7.101727f, -2.159984f),             //14
                new Vector2(-6.842444f, -3.077514f),             //15
                new Vector2(-6.695161f, -4.467651f),              //16
                new Vector2(-6.529393f, -5.421715f),             //17
                new Vector2(-6.353045f, -6.330579f),               //18
                new Vector2(-6.047956f, -7.201619f),             //19
                new Vector2(-4.917714f, -8.749873f),             //20
                new Vector2(-3.553024f, -9.724635f),             //21
                new Vector2(-2.714598f, -10.20871f),             //22
                new Vector2(-1.797196f, -10.65479f),             //23
                new Vector2(-0.3768088f, -10.9833f),             //24
                new Vector2(-0.02504795f, -5.156488f),            //25 //lip
                new Vector2(-0.7600341f, -5.003549f),            //26
                new Vector2(-1.866868f, -5.363205f),             //27
                new Vector2(-2.27355f, -5.648803f),             //28
                new Vector2(-2.242563f, -6.579151f),             //29
                new Vector2(-0.8518612f, -6.949043f),            //30
                new Vector2(-1.45163f, -2.309675f),             //31 //nose
                new Vector2(-1.836815f, -3.002033f),           //32
                new Vector2(-0.5791314f, -3.872578f),            //33
                new Vector2(-3.80265f, 0.8633401f),             //34 //eyes
                new Vector2(-4.1408f, 0.72409f),             //35    
                new Vector2(-4.18644f, 0.3733242f),             //36
                new Vector2(-3.815817f, 0.2296745f),              //37
                new Vector2(-2.885566f, 0.1571408f),              //38
                new Vector2(-2.486555f, 0.1999159f),              //39
                new Vector2(-2.408675f, 0.7724542f),             //40
                new Vector2(-2.794708f, 0.9457021f),             //41
                new Vector2(-9.126692f, 1.054292f),              //42 //ear
                new Vector2(-8.973386f, -1.763392f),              //43   
                new Vector2(-1.723354f, -6.020386f),             //44 //lip center
                new Vector2(-0.9995f, -5.908299f),             //45
            };
        #endregion

        #region Child
        public static List<Vector2> ShapeDotsChild = new List<Vector2>
            {
                //face
                new Vector2(-0.7056948f, 9.80689f),           //0
                new Vector2(-1.654669f, 9.695956f),          //1
                new Vector2(-3.1541f, 9.154608f),          //2
                new Vector2(-3.921141f, 8.746099f),          //3
                new Vector2(-4.643183f, 8.202579f),          //4
                new Vector2(-5.712764f, 7.050782f),          //5
                new Vector2(-6.033342f, 6.529766f),          //6
                new Vector2(-6.644406f, 5.280637f),          //7
                new Vector2(-6.877016f, 4.363639f),          //8
                new Vector2(-7.094626f, 2.524775f),          //9
                new Vector2(-7.120038f, 1.394236f),          //10
                new Vector2(-7.080105f, 0.5561359f),         //11
                new Vector2(-6.760277f, -0.7833537f),         //12
                new Vector2(-6.673974f, -1.591552f),         //13
                new Vector2(-6.535688f, -2.638166f),         //14
                new Vector2(-6.362336f, -3.530694f),         //15
                new Vector2(-6.177732f, -4.671041f),           //16
                new Vector2(-6.044168f, -5.181303f),         //17    
                new Vector2(-5.889578f, -5.682616f),         //18
                new Vector2(-5.634983f, -6.189272f),         //19
                new Vector2(-4.724473f, -7.437404f),         //20
                new Vector2(-3.319997f, -8.488832f),         //21
                new Vector2(-2.605252f, -8.839181f),         //22
                new Vector2(-1.801535f, -9.258749f),         //23
                new Vector2(-0.337695f, -9.527775f),         //24
                new Vector2(-0.003081355f, -5.305363f),         //25 //lip
                new Vector2(-0.3499262f, -5.177298f),         //26
                new Vector2(-1.201072f, -5.573816f),         //27
                new Vector2(-1.554831f, -5.845824f),         //28
                new Vector2(-1.590009f, -6.540767f),         //29
                new Vector2(-0.6297784f, -6.92204f),         //30
                new Vector2(-0.8569124f, -2.931683f),         //31 //nose
                new Vector2(-1.21079f, -3.357552f),         //32
                new Vector2(-0.5662344f, -4.286056f),         //33
                new Vector2(-3.337825f, -0.6208684f),         //34 //eyes
                new Vector2(-3.656225f, -0.8574543f),         //35
                new Vector2(-3.610779f, -1.224549f),         //36
                new Vector2(-3.247605f, -1.317623f),         //37
                new Vector2(-2.327342f, -1.311511f),         //38
                new Vector2(-1.949978f, -1.202075f),         //39
                new Vector2(-1.995026f, -0.8328779f),         //40
                new Vector2(-2.437889f, -0.5761855f),         //41    
                new Vector2(-8.395151f, -0.02544679f),         //42 //ear
                new Vector2(-8.422139f, -2.20578f),         //43
                new Vector2(-1.235657f, -6.23f),         //44 //lip center
                new Vector2(-0.6401863f, -6.080235f),         //45
            };
        #endregion
        #endregion

        #region Profile dots

        public List<Vector2> ShapeProfileDots = null;

        #region Fem
        public static List<Vector2> ShapeProfileDotsFem = new List<Vector2>
                                                       {
                                                           //лоб
                                                           new Vector2(1.166632f, 11.37157f),                  //0
                                                           new Vector2(2.64307f, 11.06701f),                  //1
                                                           new Vector2(3.907483f, 10.63468f),                  //2
                                                           new Vector2(5.15591f, 10.00872f),                  //3
                                                           new Vector2(6.302482f, 9.191058f),                  //4
                                                           new Vector2(7.325822f, 8.273885f),                  //5
                                                           new Vector2(7.976741f, 7.423934f),                  //6
                                                           new Vector2(8.67484f, 6.016697f),                  //7
                                                           new Vector2(9.126081f, 4.594127f),                  //8
                                                           new Vector2(9.296395f, 3.392731f),                  //9
                                                           new Vector2(9.297733f, 2.368814f),                  //10
                                                           new Vector2(9.226062f, 1.423957f),                  //11
                                                           new Vector2(9.111341f, 0.6042252f),                  //12
                                                           new Vector2(9.513948f, -0.3319094f),                   //13 //нос
                                                           new Vector2(9.795543f, -0.7262623f),                   //14
                                                           new Vector2(10.07714f, -1.207512f),                  //15
                                                           new Vector2(10.34344f, -1.601859f),                  //16
                                                           new Vector2(10.69632f, -2.19472f),                  //17
                                                           new Vector2(11.07365f, -2.740601f),                  //18
                                                           new Vector2(11.22174f, -3.334064f),                  //19
                                                           new Vector2(10.42885f, -4.293762f),                  //20 //между губами и носом
                                                           new Vector2(10.14215f, -4.455142f),                  //21
                                                           new Vector2(9.768666f, -4.573007f),                  //22
                                                           new Vector2(9.617108f, -4.789576f),                  //23
                                                           new Vector2(9.572114f, -5.069966f),                  //24 //верх губ
                                                           new Vector2(9.685264f, -5.695572f),                  //25
                                                           new Vector2(9.637376f, -5.939757f),                  //26
                                                           new Vector2(9.42307f, -6.250383f),                  //27
                                                           new Vector2(9.244642f, -6.633584f),                  //28 //низ губ
                                                           new Vector2(9.295696f, -6.867976f),                  //29
                                                           new Vector2(9.237453f, -7.241193f),                  //30
                                                           new Vector2(8.742596f, -7.980893f),                  //31 //подбородок
                                                           new Vector2(8.750419f, -8.511849f),                  //32
                                                           new Vector2(8.827f, -9.104196f),                  //33
                                                           new Vector2(8.711728f, -9.7543f),                  //34
                                                           new Vector2(8.204303f, -10.50308f),                  //35
                                                           new Vector2(7.316035f, -10.87344f),                  //36
                                                           new Vector2(6.362537f, -10.92896f),                  //37
                                                           new Vector2(5.623556f, -10.94081f),                  //38
                                                           new Vector2(4.919567f, -10.92828f),                  //39
                                                           new Vector2(4.344431f, -10.93698f),                  //40
                                                           new Vector2(3.758614f, -11.02614f),                  //41
                                                           new Vector2(3.2191f, -11.20252f),                  //42
                                                       };
        #endregion

        #region Male
        public static List<Vector2> ShapeProfileDotsMale = new List<Vector2>
                                                       {
                                                           //лоб
                                                           new Vector2(1.166632f, 11.81903f),              //0
                                                           new Vector2(2.404973f, 11.47721f),              //1
                                                           new Vector2(3.750115f, 11.05489f),              //2
                                                           new Vector2(5.133953f, 10.3068f),              //3
                                                           new Vector2(6.305389f, 9.477346f),              //4
                                                           new Vector2(7.274693f, 8.402552f),              //5
                                                           new Vector2(8.182655f, 7.036903f),             //6
                                                           new Vector2(8.62605f, 5.984096f),             //7
                                                           new Vector2(8.999768f, 4.594015f),             //8
                                                           new Vector2(9.259664f, 3.623764f),              //9
                                                           new Vector2(9.343696f, 2.7931f),              //10
                                                           new Vector2(9.372128f, 2.095729f),             //11
                                                           new Vector2(9.309289f, 1.599562f),              //12
                                                           new Vector2(9.595323f, 0.612157f),              //13 //нос
                                                           new Vector2(9.957355f, -0.1614273f),              //14
                                                           new Vector2(10.50295f, -1.00694f),                 //15
                                                           new Vector2(10.89538f, -1.666215f),                 //16
                                                           new Vector2(11.37601f, -2.39949f),                 //17
                                                           new Vector2(11.45644f, -2.998555f),                 //18
                                                           new Vector2(11.3032f, -3.511222f),                 //19
                                                           new Vector2(10.57758f, -3.943785f),                 //20 //между губами и носом
                                                           new Vector2(10.21409f, -4.081529f),             //21
                                                           new Vector2(9.976532f, -4.237591f),             //22
                                                           new Vector2(9.766545f, -4.500658f),             //23
                                                           new Vector2(9.781264f, -4.778694f),             //24 //верх губ
                                                           new Vector2(9.935394f, -5.415217f),             //25
                                                           new Vector2(9.786812f, -5.697416f),             //26
                                                           new Vector2(9.608873f, -5.924529f),             //27
                                                           new Vector2(9.471186f, -6.226395f),             //28 //низ губ
                                                           new Vector2(9.517109f, -6.418077f),             //29
                                                           new Vector2(9.533085f, -6.680507f),             //30
                                                           new Vector2(9.139763f, -7.321826f),             //31 //подбородок
                                                           new Vector2(9.01193f, -7.729424f),             //32
                                                           new Vector2(9.131616f, -8.317992f),             //33
                                                           new Vector2(9.089453f, -8.974551f),             //34
                                                           new Vector2(8.872182f, -9.701309f),             //35
                                                           new Vector2(8.411968f, -10.23561f),             //36
                                                           new Vector2(7.748407f, -10.52055f),             //37
                                                           new Vector2(7.002545f, -10.69977f),             //38
                                                           new Vector2(6.102588f, -10.78686f),             //39
                                                           new Vector2(5.036375f, -10.92699f),             //40
                                                           new Vector2(4.100905f, -11.05619f),             //41
                                                           new Vector2(3.435307f, -11.28881f),             //42
                                                       };
        #endregion

        #region Child
        public static List<Vector2> ShapeProfileDotsChild = new List<Vector2>
                                                       {
                                                           //лоб
                                                           new Vector2(1.279177f, 9.704621f),              //0
                                                           new Vector2(2.437461f, 9.428654f),               //1
                                                           new Vector2(4.007101f, 8.924129f),               //2
                                                           new Vector2(5.15591f, 8.367063f),              //3
                                                           new Vector2(6.157748f, 7.70598f),               //4
                                                           new Vector2(6.883566f, 6.910417f),              //5
                                                           new Vector2(7.609283f, 6.085816f),              //6
                                                           new Vector2(8.222853f, 4.929422f),               //7
                                                           new Vector2(8.523155f, 3.679504f),               //8
                                                           new Vector2(8.579426f, 2.371554f),               //9
                                                           new Vector2(8.508477f, 1.420458f),               //10
                                                           new Vector2(8.392188f, 0.5479957f),              //11
                                                           new Vector2(8.206697f, -0.1543421f),              //12
                                                           new Vector2(8.224838f, -1.120063f),                //13 //нос
                                                           new Vector2(8.354287f, -1.49827f),                //14
                                                           new Vector2(8.600957f, -1.992363f),             //15
                                                           new Vector2(8.881349f, -2.384704f),              //16
                                                           new Vector2(9.234231f, -2.81789f),              //17
                                                           new Vector2(9.576634f, -3.170175f),              //18
                                                           new Vector2(9.628407f, -3.557666f),             //19
                                                           new Vector2(9.208611f, -4.11922f),              //20 //между губами и носом
                                                           new Vector2(8.911641f, -4.247954f),              //21
                                                           new Vector2(8.666619f, -4.426606f),              //22
                                                           new Vector2(8.69748f, -4.795339f),               //23
                                                           new Vector2(8.811213f, -5.077642f),              //24 //верх губ
                                                           new Vector2(8.835202f, -5.593185f),              //25
                                                           new Vector2(8.717448f, -5.755278f),              //26
                                                           new Vector2(8.565697f, -5.888942f),              //27
                                                           new Vector2(8.466325f, -6.234066f),             //28 //низ губ
                                                           new Vector2(8.541068f, -6.486726f),              //29
                                                           new Vector2(8.455363f, -6.733421f),              //30
                                                           new Vector2(8.136765f, -7.162772f),              //31 //подбородок
                                                           new Vector2(8.032103f, -7.3828f),             //32
                                                           new Vector2(8.005236f, -7.72573f),               //33
                                                           new Vector2(8.048237f, -8.242459f),              //34
                                                           new Vector2(7.923965f, -8.784634f),               //35
                                                           new Vector2(7.672922f, -9.18424f),               //36
                                                           new Vector2(7.134603f, -9.405045f),              //37
                                                           new Vector2(6.411414f, -9.489067f),              //38
                                                           new Vector2(5.499667f, -9.509139f),              //39
                                                           new Vector2(4.729064f, -9.505907f),             //40
                                                           new Vector2(4.03358f, -9.510821f),               //41
                                                           new Vector2(3.463454f, -9.82539f),              //42
                                                       };
        #endregion
        #endregion

        #region Indices
        public static List<int> ShapeIndices = new List<int>()
        {
            8,77,89,
            8,9,77,
            9,99,77,
            9,10,99,
            10,100,99,
            10,101,100,
            10,11,101,
            11,12,101,
            12,13,101,
            13,90,101,
            13,14,90,
            14,15,90,
            15,91,90,
            15,16,91,
            16,17,91,
            17,18,91,
            18,19,91,
            19,20,91,
            20,21,91,
            21,22,91,
            22,92,91,
            22,23,92,
            23,93,92,
            23,24,93,
            24,25,93,
            25,26,93,
            26,94,93,
            26,27,94,
            27,95,94,
            27,28,95,
            28,96,95,
            28,29,96,
            29,30,96,
            30,31,96,
            31,97,96,
            31,79,97,
            31,80,79,
            31,81,80,
            31,122,81,
            31,32,124,
            31,122,123,
            31,123,124,
            32,33,124,
            33,34,124,
            34,35,124,
            35,36,124,
            36,125,124,
            36,37,125,
            37,38,125,
            38,126,125,
            38,127,126,
            38,39,127,
            39,40,127,
            40,41,127,
            41,42,127,
            42,128,127,
            42,43,128,
            43,129,128,
            43,44,129,
            44,45,129,
            45,46,129,
            46,47,129,
            47,130,129,
            47,131,130,
            47,48,131,
            48,49,131,
            49,114,131,
            49,50,114,
            50,51,114,
            54,53,114,
            54,114,115,
            54,115,116,
            53,52,114,
            52,51,114,
            54,85,116,
            54,86,85,
            54,87,86,
            54,109,87,
            54,108,109,
            54,55,108,
            55,56,108,
            56,57,108,
            57,107,108,
            57,58,107,
            58,106,107,
            58,59,106,
            59,105,106,
            59,60,105,
            60,61,105,
            61,62,105,
            62,104,105,
            62,63,104,
            63,103,104,
            63,64,103,
            64,65,103,
            65,66,103,
            66,67,103,
            67,68,103,
            68,69,103,
            69,70,103,
            70,102,103,
            70,71,102,
            71,72,102,
            72,113,102,
            72,73,113,
            73,74,113,
            74,75,113,
            75,112,113,
            75,111,112,
            75,76,111,
            76,89,111,
            76,8,89,
            79,98,97,
            79,99,98,
            79,78,99,
            78,77,99,
            87,109,110,
            87,110,111,
            88,87,111,
            88,111,89,
            122,121,81,
            121,82,81,
            121,120,82,
            120,83,82,
            120,119,83,
            119,118,83,
            118,84,83,
            118,117,84,
            117,85,84,
            117,116,85,
            //нос
            84,85,86,
            84,86,87,
            84,87,88,
            84,88,83,
            83,88,78,
            82,83,78,
            82,78,79,
            82,79,80,
            80,81,82,
            78,88,77,
            88,89,77,
            //левое ухо
            0,1,24,
            1,25,24,
            1,26,25,
            1,2,26,
            2,27,26,
            2,28,27,
            2,29,28,
            2,3,29,
            3,30,29,
            3,31,30,
            //правое ухо
            6,7,61,
            6,61,60,
            6,60,59,
            5,6,59,
            5,59,58,
            5,58,57,
            5,57,56,
            4,5,56,
            4,56,55,
            4,55,54,
            //губы
            124,125,132,
            124,132,123,
            125,126,132,
            126,133,132,
            126,127,133,
            127,134,133,
            127,128,134,
            128,129,134,
            129,135,134,
            129,130,135,
            130,136,135,
            130,131,136,
            131,114,136,
            114,115,136,
            136,115,116,
            116,135,136,
            116,117,135,
            117,118,135,
            118,134,135,
            118,119,134,
            119,120,134,
            120,133,134,
            120,121,133,
            121,122,133,
            122,132,133,
            122,123,132,
            //левый глаз
            98,99,100,
            98,100,101,
            97,98,101,
            96,97,101,
            90,96,101,
            90,91,96,
            91,95,96,
            91,92,94,
            91,94,95,
            92,93,94,
            //правый глаз
            104,106,105,
            103,107,106,
            106,104,103,
            103,102,108,
            108,107,103,
            102,113,108,
            113,109,108,
            113,112,110,
            110,109,113,
            111,110,112
        };

        public List<int> GetFaceIndexes()
        {
            var result = new List<int>();
            for (var i = 0; i <= 76;i++ )
                result.Add(i);
            return result;
        }
        public List<int> GetMouthIndexes()
        {
            var result = new List<int>();
            for (var i = 114; i <= 136; i++)
                result.Add(i);
            return result;
        }
        public List<int> GetLeftEyeIndexes()
        {
            var result = new List<int>();
            for (var i = 90; i <= 101; i++)
                result.Add(i);
            return result;
        }
        public List<int> GetRightEyeIndexes()
        {
            var result = new List<int>();
            for (var i = 102; i <= 113; i++)
                result.Add(i);
            return result;
        }
        public List<int> GetNoseIndexes()
        {
            var result = new List<int>();
            for (var i = 77; i <= 89; i++)
                result.Add(i);
            return result;
        }


        #endregion

        private Dictionary<int, int> dotsDictionary = new Dictionary<int, int>();
        private Dictionary<int, int> profileDotsDictionary = new Dictionary<int, int>(); 
        private Dictionary<int, List<int>> linkedPoints = new Dictionary<int, List<int>>();

        public TexturingInfo ShapeInfo = new TexturingInfo();
        public TexturingInfo ShapeProfileInfo = new TexturingInfo();

        public HeadMeshesController headMeshesController;
        private Vector2 GetMirrored(int index)
        {
            var res = ShapeDots[index];
            res.X *= -1.0f;
            return res;
        }

        public List<AutodotsShapeRect> Rects = new List<AutodotsShapeRect>();
        public List<AutodotsShapeRect> ProfileRects = new List<AutodotsShapeRect>();

        public void SetType(int type)
        {
            switch (type)
            {
                case 1:
                    ShapeProfileDots = ShapeProfileDotsMale;
                    ShapeDots = ShapeDotsMale;
                    break;
                case 2:
                    ShapeProfileDots = ShapeProfileDotsChild;
                    ShapeDots = ShapeDotsChild;
                    break;
                default:
                    ShapeProfileDots = ShapeProfileDotsFem;
                    ShapeDots = ShapeDotsFem;
                    return;
            }
        }

        public void Transform(HeadPoint[] dots, bool needShape = false)
        {
            if (!needShape)
            {
                foreach (var r in Rects)
                {
                    r.Transform(ref dots[r.A].Value, ref dots[r.B].Value);
                    for (var i = 0; i < r.Points.Length; i++)
                        ShapeInfo.Points[r.ShapeIndices[i]].Value = r.Points[i];
                }
                return;
            }
            for (var i = 0; i < dots.Length; i++)
                Transform(dots[i].Value, i, false);
            headMeshesController.UpdateShape(ref ShapeInfo);
        }

        private static bool Vector2Equals(ref Vector2 a, ref Vector2 b)
        {
            const float delta = 0.00001f;
            return Math.Abs(a.X - b.X) < delta && Math.Abs(a.Y - b.Y) < delta;
        }

        public void Transform(Vector2 newPosition, int index, bool needShape = true)
        {
            var rects = Rects.Where(r => r.A.Equals(index) || r.B.Equals(index)).ToList();
            if (rects.Count == 0)
                return;
            var point = rects.First().A == index ? rects.First().Points.First() : rects.First().Points.Last();
            var ind = ShapeInfo.Points.ToList().FindIndex(p => Vector2Equals(ref p.Value, ref point));
            if (ind < 0)
                return;
            ShapeInfo.Points[ind].Value = newPosition;
            foreach (var rect in rects)
            {
                Vector2 a, b;
                if (rect.A.Equals(index))
                {
                    a = newPosition;
                    b = rect.Points.Last();
                }
                else
                {
                    a = rect.Points.First();
                    b = newPosition;
                }
                rect.Transform(ref a, ref b);
                for (var i = 1; i < rect.Points.Length - 1; i++)
                {
                    var pos = rect.Points[i];
                    var idx = rect.ShapeIndices[i];
                    ShapeInfo.Points[idx].Value = pos;
                }
            }
            if (needShape)
                headMeshesController.UpdateShape(ref ShapeInfo);
        }

        public void Transform(MeshPartType type, List<Vector2> points, Vector2 center)
        {
            var movedPoints = new List<int>();
            var linkedIndices = new Dictionary<int, int>();
            var rects = Rects.Where(r => r.Type == type).ToList();
            foreach (var rect in rects)
            {
                for (var i = 0; i < rect.Points.Length; i++)
                {
                    var idx = rect.ShapeIndices[i];
                    if (movedPoints.Contains(idx))
                        continue;
                    movedPoints.Add(idx);
                }
                if (rect.LinkedShapeRect != null)
                {
                    linkedIndices.Add(rect.ShapeIndices.First(), rect.LinkedShapeRect.ShapeIndices.First());
                    linkedIndices.Add(rect.ShapeIndices.Last(), rect.LinkedShapeRect.ShapeIndices.Last());
                }
            }
            switch (type)
            {
                case MeshPartType.LEye:
                case MeshPartType.REye:
                case MeshPartType.Head:
                    foreach (var key in movedPoints)
                    {
                        var p = ShapeInfo.Points[key].Value;
                        float ua, ub;
                        Vector2 p0, p1, target;
                        for (int i = 0, j = points.Count - 1; i < points.Count; j = i, i++)
                        {
                            p0 = points[j];
                            p1 = points[i];
                            if (GetUaUb(ref center, ref p, ref p0, ref p1, out ua, out ub) && ub > 0.0f && ub < 1.0f && ua > 0.0f)
                            {
                                target = center + (p - center) * ua;
                                ShapeInfo.Points[key].Value = target;
                                break;
                            }
                        }
                    }
                    break;
                case MeshPartType.Nose:
                    if ((points[points.Count - 1] - points[0]).X < 0.0f)
                        points.Reverse();
                    var plist = movedPoints.Select(i => ShapeInfo.Points[i].Value).ToList();
                    plist = TransformLine(plist, points);
                    for (int i = 0; i < movedPoints.Count; i++)
                    {
                        var key = movedPoints[i];
                        var p = plist[i];
                        ShapeInfo.Points[key].Value = p;
                    }
                    break;

                case MeshPartType.Lip:
                    #region Lip
                    {
                        if (points.Count < 5)
                            return;
                        var lipPoints = new List<Vector2>();
                        var indices = new List<int>();
                        var pointnsDict = new Dictionary<Vector2, int>(new VectorEqualityComparer());
                        var centerLine = false;
                        for (int i = 0; i < points.Count; i++)
                        {
                            var p = points[i];
                            if (i > 0 && !centerLine && p.Equals(points[0]))
                            {
                                centerLine = true;
                                continue;
                            }
                            int index;
                            if (!pointnsDict.TryGetValue(p, out index))
                            {
                                index = lipPoints.Count;
                                pointnsDict.Add(p, index);
                                lipPoints.Add(p);
                            }
                            indices.Add(index);
                        }

                        var lipLists = new List<Vector2>[3];
                        for (int i = 0; i < 3; i++)
                            lipLists[i] = new List<Vector2>();
                        for (int i = 0; i < indices.Count; i++)
                            if (indices[i] < i)
                            {
                                var first = indices[i];
                                var last = indices[indices.Count - 1];
                                for (int j = first; j <= last; j++)
                                    lipLists[0].Add(lipPoints[indices[j]]);
                                for (int j = last; j < i; j++)
                                    lipLists[1].Add(lipPoints[indices[j]]);
                                for (int j = 0; j <= first; j++)
                                    lipLists[1].Add(lipPoints[indices[j]]);
                                for (int j = i; j < indices.Count; j++)
                                    lipLists[2].Add(lipPoints[indices[j]]);
                                break;
                            }
                        for (int i = 0; i < 3; i++)
                            if (lipLists[i].Count == 0)
                                return;
                        var lipCenter = lipLists[2];
                        var c = (lipCenter[0] + lipPoints[lipCenter.Count - 1]) * 0.5f;
                        var c0 = new Vector2(c.X, c.Y + 1000.0f);
                        var c1 = new Vector2(c.X, c.Y - 1000.0f);
                        for (int i = 2; i > 0; i--)
                        {
                            var list = lipLists[i];
                            for (int j = 0; j < list.Count - 1; j++)
                            {
                                var p0 = list[j];
                                var p1 = list[j + 1];
                                float ua, ub;
                                if (GetUaUb(ref p0, ref p1, ref c0, ref c1, out ua, out ub) &&
                                    ua > 0.0f && ua < 1.0f && ub > 0.0f && ub < 1.0f)
                                {
                                    var p = p0 + (p1 - p0) * ua;
                                    if (i == 2)
                                        c = p;
                                    else if (p.Y >= c.Y)
                                    {
                                        lipLists[1] = lipLists[0];
                                        lipLists[0] = list;
                                    }
                                    break;
                                }
                            }
                        }
                        var tmpIndices = new[] { 3, 4, 2 };
                        var idx = 0;
                        var sourcePoints = new List<Vector2>();
                        var sourceIndices = new List<int>();
                        for (int i = 0; i < 3; i++)
                        {
                            sourcePoints.Clear();
                            sourceIndices.Clear();
                            for (int j = 0; j < tmpIndices[i]; j++, idx++)
                            {
                                var rect = rects[idx];
                                sourcePoints.AddRange(rect.Points);
                                sourceIndices.AddRange(rect.ShapeIndices);
                            }
                            var list = lipLists[i];
                            if ((sourcePoints[sourcePoints.Count - 1].X - sourcePoints[0].X) * (list[list.Count - 1].X - list[0].X) < 0.0f)
                                list.Reverse();
                            sourcePoints = TransformLine(sourcePoints, list);
                            for (int j = 0; j < sourcePoints.Count; j++)
                            {
                                var id = sourceIndices[j];
                                if (movedPoints.Contains(id))
                                {
                                    var p = sourcePoints[j];
                                    ShapeInfo.Points[id].Value = p;
                                    movedPoints.Remove(id);
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case MeshPartType.ProfileBottom:
                case MeshPartType.ProfileTop:
                    #region Profile
                    {
                        var index = 0;
                        bool isFirst = true;
                        movedPoints.Clear();
                        foreach (var rect in ProfileRects)
                        {
                            if (rect.LinkedShapeRect == null || rect.Type != type)
                                continue;
                            //Подгоняем основные точки по Y
                            Vector2 a = rect.Points[0], b = rect.Points.Last();

                            var v = new Vector2(1.0f, 0.0f);
                            var a1 = a + v;
                            var b1 = b + v;
                            //Подгоняем точки по Z
                            float? az = null, bz = null;
                            var tmpPoints = new List<Vector2>();
                            if (isFirst)
                            {
                                az = a.X;
                                tmpPoints.Add(a);
                            }
                            if (rect.IsLast)
                                bz = b.X;
                            //Идем от index и ищем пересечение линии [i, i + 1] с прямой, парралельной oX проходящей через a.Y и b.Y
                            //Если дошли до конца и не нашли - что-то пошло не так, прекращаем все
                            for (int i = index; i < points.Count - 1; i++)
                            {
                                float ua, ub;
                                var p0 = points[i];
                                var p1 = points[i + 1];
                                if (az == null && GetUaUb(ref p0, ref p1, ref a, ref a1, out ua, out ub) && ua > 0.0f && ua < 1.0f)
                                {
                                    var tmp = p0 + (p1 - p0) * ua;
                                    tmpPoints.Add(tmp);
                                    az = tmp.X;
                                }
                                if (az != null)
                                {
                                    if (rect.IsLast)
                                    {
                                        for (int j = i + 1; j < points.Count - 1; j++)
                                            tmpPoints.Add(points[j]);
                                        tmpPoints.Add(b);
                                        break;
                                    }
                                    if (i != index && p0.Y < a.Y)
                                        tmpPoints.Add(points[i]);
                                    if (GetUaUb(ref p0, ref p1, ref b, ref b1, out ua, out ub) && ua > 0.0f && ua < 1.0f)
                                    {
                                        var tmp = p0 + (p1 - p0) * ua;
                                        tmpPoints.Add(tmp);
                                        bz = tmp.X;
                                        index = i;
                                        break;
                                    }
                                }
                            }
                            if (az == null || bz == null)
                                return;
                            a.X = az.Value;
                            b.X = bz.Value;
                            rect.Transform(ref a, ref b);
                            //Строим линию
                            rect.Points = TransformLine(rect.Points.ToList(), tmpPoints).ToArray();
                            //Обновляем точки для шейпа
                            var count = rect.IsLast ? rect.Points.Length - 1 : rect.Points.Length;
                            var start = isFirst ? 1 : 0;
                            for (var i = start; i < count; i++)
                            {
                                var idx = rect.ShapeIndices[i];
                                if (movedPoints.Contains(idx))
                                    continue;
                                movedPoints.Add(idx);
                                ShapeProfileInfo.Points[idx].Value = rect.Points[i];
                            }
                            isFirst = false;
                        }
                        headMeshesController.UpdateProfileShape(ref ShapeProfileInfo);
                        headMeshesController.UpdateNormals();
                        //headMeshesController.UpdateShape(ref ShapeInfo);
                        return;
                    }
                    #endregion
            }

            foreach (var rect in rects)
            {
                if (rect.LinkedShapeRect != null)
                {
                    Vector2 a = ShapeInfo.Points[rect.ShapeIndices.First()].Value, b = ShapeInfo.Points[rect.ShapeIndices.Last()].Value;
                    a = rect.LinkedShapeRect.Points.First() + a - rect.Points.First();
                    b = rect.LinkedShapeRect.Points.Last() + b - rect.Points.Last();

                    rect.LinkedShapeRect.Transform(ref a, ref b, false);
                    for (var i = 0; i < rect.LinkedShapeRect.Points.Length; i++)
                    {
                        var pos = rect.LinkedShapeRect.Points[i];
                        var idx = rect.LinkedShapeRect.ShapeIndices[i];
                        ShapeInfo.Points[idx].Value = pos;
                    }
                }

                for (var i = 0; i < rect.Points.Length; i++)
                {
                    var idx = rect.ShapeIndices[i];
                    rect.Points[i] = ShapeInfo.Points[idx].Value;
                }
                rect.Points = rect.Points;
            }

            headMeshesController.UpdateShape(ref ShapeInfo);
        }

        private static List<Vector2> TransformLine(IList<Vector2> basePoints, IList<Vector2> targetPoints)
        {
            var result = new List<Vector2>();
            //1. Нужно найти длину каждой линии
            var lengthBase = 0.0f;
            var lengthTransform = 0.0f;
            for (int i = 1; i < basePoints.Count; i++)
                lengthBase += (basePoints[i] - basePoints[i - 1]).Length;
            for (int i = 1; i < targetPoints.Count; i++)
                lengthTransform += (targetPoints[i] - targetPoints[i - 1]).Length;
            //2. Находим отношение
            float k = lengthTransform / lengthBase;
            //3. Находим соответствующую позицию каждой точки на финальной линии
            lengthBase = 0.0f;
            lengthTransform = 0.0f;
            int lastPoint = 0;
            int prevPoint = -1;
            for (int i = 0; i < basePoints.Count; i++)
            {
                if (prevPoint > -1)
                    lengthBase += (basePoints[i] - basePoints[prevPoint]).Length * k;
                prevPoint = i;
                for (int j = lastPoint; j < targetPoints.Count; j++)
                {
                    var last = j == targetPoints.Count - 1;
                    var l = last ? 100.0f : (targetPoints[j + 1] - targetPoints[j]).Length;
                    if (lengthTransform + l > lengthBase)
                    {
                        lastPoint = j;
                        if (last)
                            result.Add(targetPoints[targetPoints.Count - 1]);
                        else
                        {
                            var dir = (targetPoints[j + 1] - targetPoints[j]).Normalized();
                            result.Add(dir * (lengthBase - lengthTransform) + targetPoints[lastPoint]);
                        }
                        break;
                    }
                    lengthTransform += l;
                }
            }
            return result;
        }

        public static bool GetUaUb(ref Vector2 a0, ref Vector2 a1, ref Vector2 b0, ref Vector2 b1, out float ua, out float ub)
        {
            ua = 0.0f;
            ub = 0.0f;
            var z = (b1.Y - b0.Y) * (a1.X - a0.X) - (b1.X - b0.X) * (a1.Y - a0.Y);
            if (z == 0.0f)
                return false;
            //// denominator
            ua = ((b1.X - b0.X) * (a0.Y - b0.Y) - (b1.Y - b0.Y) * (a0.X - b0.X)) / z;
            ub = ((a1.X - a0.X) * (a0.Y - b0.Y) - (a1.Y - a0.Y) * (a0.X - b0.X)) / z;
            return true;
        }

        public void ResetPoints(MeshPartType type)
        {
            if (type == MeshPartType.None)
                return;
            var movedPoints = new Dictionary<int, Vector2>();
            var rects = Rects.Where(r => r.Type == type).ToList();
            var linked = rects.Where(r => r.LinkedShapeRect != null).Select(r => r.LinkedShapeRect).ToList();
            rects.AddRange(linked);
            foreach (var rect in rects)
            {
                for (var i = 0; i < rect.Points.Length; i++)
                {
                    var idx = rect.ShapeIndices[i];
                    if (movedPoints.ContainsKey(idx))
                    {
                        rect.Points[i] = movedPoints[idx];
                        continue;
                    }
                    movedPoints.Add(idx, rect.OriginalPoints[i]);
                    var dv = rect.OriginalPoints[i] - rect.Points[i];
                    if (Math.Abs(dv.LengthSquared) < 0.000001f)
                        continue;
                    rect.Points[i] = rect.OriginalPoints[i];
                    ShapeInfo.Points[idx].Value = rect.OriginalPoints[i];
                }
                rect.Points = rect.Points;
            }
            headMeshesController.UpdateShape(ref ShapeInfo);
            headMeshesController.UpdateNormals();
        }

        public void UpdateProfileLines()
        {
            foreach (var rect in ProfileRects.Where(rect => rect.LinkedShapeRect != null))
            {
                //Подгоняем основные точки по Y
                Vector2 a = rect.Points[0], b = rect.Points.Last();

                var list = linkedPoints[rect.A];
                a.Y = 0.0f;
                foreach (var l in list)
                    a.Y += ShapeInfo.Points[l].Value.Y;
                a.Y /= list.Count;

                list = linkedPoints[rect.B];
                b.Y = 0.0f;
                foreach (var l in list)
                    b.Y += ShapeInfo.Points[l].Value.Y;
                b.Y /= list.Count;
                rect.Transform(ref a, ref b);
                a.X = rect.LinkedShapeRect.Points[0].X;
                b.X = rect.LinkedShapeRect.Points.Last().X;
                rect.LinkedShapeRect.Transform(ref a, ref b);

                for (var i = 0; i < rect.ShapeIndices.Length; i++)
                    ShapeProfileInfo.Points[rect.ShapeIndices[i]].Value = rect.Points[i];
                for (var i = 0; i < rect.LinkedShapeRect.ShapeIndices.Length; i++)
                    ShapeProfileInfo.Points[rect.LinkedShapeRect.ShapeIndices[i]].Value = rect.LinkedShapeRect.Points[i];
            }
        }

        private List<HeadPoint> defaultDots = null;

        public void Initialise(List<HeadPoint> baseDots)
        {
            defaultDots = baseDots;
            var shapePoints = new List<Vector2>();
            Rects.Clear();
            dotsDictionary.Clear();
            //ear
            var lEar = new AutodotsShapeRect
            {
                A = 47,
                B = 48,
                Type = MeshPartType.LEar
            }.Initialise(new[] { baseDots[47].Value, ShapeDots[42], ShapeDots[43], baseDots[48].Value }, ref shapePoints, true);
            //temp
            dotsDictionary.Add(42, lEar.ShapeIndices[1]);
            dotsDictionary.Add(43, lEar.ShapeIndices[2]);
            //temp
            Rects.Add(lEar);
            var rEar = new AutodotsShapeRect
            {
                A = 50,
                B = 49,
                Type = MeshPartType.REar
            }.Initialise(new[] { baseDots[50].Value, GetMirrored(43), GetMirrored(42), baseDots[49].Value }, ref shapePoints, true);
            Rects.Add(rEar);
            //head
            Rects.Add(new AutodotsShapeRect
            {
                A = 0,
                B = 3,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[0].Value, ShapeDots[0], ShapeDots[1], baseDots[3].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(0, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(1, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 3,
                B = 4,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[3].Value, ShapeDots[2], ShapeDots[3], ShapeDots[4], baseDots[4].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(2, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(3, Rects.Last().ShapeIndices[2]);
            dotsDictionary.Add(4, Rects.Last().ShapeIndices[3]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 4,
                B = 5,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[4].Value, ShapeDots[5], ShapeDots[6], baseDots[5].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(5, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(6, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 5,
                B = 6,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[5].Value, ShapeDots[7], ShapeDots[8], baseDots[6].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(7, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(8, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 6,
                B = 7,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[6].Value, ShapeDots[9], ShapeDots[10], ShapeDots[11], baseDots[7].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(9, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(10, Rects.Last().ShapeIndices[2]);
            dotsDictionary.Add(11, Rects.Last().ShapeIndices[3]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 7,
                B = 8,
                Type = MeshPartType.Head,
                LinkedShapeRect = lEar
            }.Initialise(new[] { baseDots[7].Value, ShapeDots[12], ShapeDots[13], ShapeDots[14], ShapeDots[15], baseDots[8].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(12, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(13, Rects.Last().ShapeIndices[2]);
            dotsDictionary.Add(14, Rects.Last().ShapeIndices[3]);
            dotsDictionary.Add(15, Rects.Last().ShapeIndices[4]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 8,
                B = 9,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[8].Value, ShapeDots[16], ShapeDots[17], ShapeDots[18], ShapeDots[19], baseDots[9].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(16, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(17, Rects.Last().ShapeIndices[2]);
            dotsDictionary.Add(18, Rects.Last().ShapeIndices[3]);
            dotsDictionary.Add(19, Rects.Last().ShapeIndices[4]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 9,
                B = 10,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[9].Value, ShapeDots[20], baseDots[10].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(20, Rects.Last().ShapeIndices[1]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 10,
                B = 11,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[10].Value, ShapeDots[21], ShapeDots[22], ShapeDots[23], baseDots[11].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(21, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(22, Rects.Last().ShapeIndices[2]);
            dotsDictionary.Add(23, Rects.Last().ShapeIndices[3]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 11,
                B = 33,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[11].Value, ShapeDots[24], GetMirrored(24), baseDots[33].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(24, Rects.Last().ShapeIndices[1]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 33,
                B = 32,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[33].Value, GetMirrored(23), GetMirrored(22), GetMirrored(21), baseDots[32].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 32,
                B = 31,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[32].Value, GetMirrored(20), baseDots[31].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 31,
                B = 30,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[31].Value, GetMirrored(19), GetMirrored(18), GetMirrored(17), GetMirrored(16), baseDots[30].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 30,
                B = 29,
                Type = MeshPartType.Head,
                LinkedShapeRect = rEar
            }.Initialise(new[] { baseDots[30].Value, GetMirrored(15), GetMirrored(14), GetMirrored(13), GetMirrored(12), baseDots[29].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 29,
                B = 28,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[29].Value, GetMirrored(11), GetMirrored(10), GetMirrored(9), baseDots[28].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 28,
                B = 27,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[28].Value, GetMirrored(8), GetMirrored(7), baseDots[27].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 27,
                B = 26,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[27].Value, GetMirrored(6), GetMirrored(5), baseDots[26].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 26,
                B = 25,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[26].Value, GetMirrored(4), GetMirrored(3), GetMirrored(2), baseDots[25].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 25,
                B = 0,
                Type = MeshPartType.Head
            }.Initialise(new[] { baseDots[25].Value, GetMirrored(1), GetMirrored(0), baseDots[0].Value }, ref shapePoints, true, true));
            //nose
            Rects.Add(new AutodotsShapeRect
            {
                A = 18,
                B = 19,
            }.Initialise(new[] { baseDots[18].Value, ShapeDots[31], ShapeDots[32], baseDots[19].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(31, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(32, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 19,
                B = 20,
                Type = MeshPartType.Nose
            }.Initialise(new[] { baseDots[19].Value, baseDots[20].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 20,
                B = 2,
                Type = MeshPartType.Nose
            }.Initialise(new[] { baseDots[20].Value, ShapeDots[33], baseDots[2].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(33, Rects.Last().ShapeIndices[1]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 2,
                B = 42,
                Type = MeshPartType.Nose
            }.Initialise(new[] { baseDots[2].Value, GetMirrored(33), baseDots[42].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 42,
                B = 41,
                Type = MeshPartType.Nose
            }.Initialise(new[] { baseDots[42].Value, baseDots[41].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 41,
                B = 40,
            }.Initialise(new[] { baseDots[41].Value, GetMirrored(32), GetMirrored(31), baseDots[40].Value }, ref shapePoints, true));
            //left eye
            Rects.Add(new AutodotsShapeRect
            {
                A = 22,
                B = 21,
                Type = MeshPartType.LEye
            }.Initialise(new[] { baseDots[22].Value, ShapeDots[34], ShapeDots[35], baseDots[21].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(34, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(35, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 21,
                B = 24,
                Type = MeshPartType.LEye
            }.Initialise(new[] { baseDots[21].Value, ShapeDots[36], ShapeDots[37], baseDots[24].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(36, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(37, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 24,
                B = 23,
                Type = MeshPartType.LEye
            }.Initialise(new[] { baseDots[24].Value, ShapeDots[38], ShapeDots[39], baseDots[23].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(38, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(39, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 23,
                B = 22,
                Type = MeshPartType.LEye
            }.Initialise(new[] { baseDots[23].Value, ShapeDots[40], ShapeDots[41], baseDots[22].Value }, ref shapePoints, true, true));
            //temp
            dotsDictionary.Add(40, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(41, Rects.Last().ShapeIndices[2]);
            //temp
            //right eye
            Rects.Add(new AutodotsShapeRect
            {
                A = 44,
                B = 43,
                Type = MeshPartType.REye
            }.Initialise(new[] { baseDots[44].Value, GetMirrored(34), GetMirrored(35), baseDots[43].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 43,
                B = 46,
                Type = MeshPartType.REye
            }.Initialise(new[] { baseDots[43].Value, GetMirrored(36), GetMirrored(37), baseDots[46].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 46,
                B = 45,
                Type = MeshPartType.REye
            }.Initialise(new[] { baseDots[46].Value, GetMirrored(38), GetMirrored(39), baseDots[45].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 45,
                B = 44,
                Type = MeshPartType.REye
            }.Initialise(new[] { baseDots[45].Value, GetMirrored(40), GetMirrored(41), baseDots[44].Value }, ref shapePoints, true, true));
            //lip top
            Rects.Add(new AutodotsShapeRect
            {
                A = 37,
                B = 38,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[37].Value, GetMirrored(28), GetMirrored(27), baseDots[38].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 38,
                B = 16,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[38].Value, GetMirrored(26), ShapeDots[25], ShapeDots[26], baseDots[16].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(25, Rects.Last().ShapeIndices[2]);
            dotsDictionary.Add(26, Rects.Last().ShapeIndices[3]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 16,
                B = 15,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[16].Value, ShapeDots[27], ShapeDots[28], baseDots[15].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(27, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(28, Rects.Last().ShapeIndices[2]);
            //temp
            //lip bottom
            Rects.Add(new AutodotsShapeRect
            {
                A = 15,
                B = 17,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[15].Value, ShapeDots[29], baseDots[17].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(29, Rects.Last().ShapeIndices[1]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 17,
                B = 1,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[17].Value, ShapeDots[30], baseDots[1].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(30, Rects.Last().ShapeIndices[1]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 1,
                B = 39,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[1].Value, GetMirrored(30), baseDots[39].Value }, ref shapePoints));
            Rects.Add(new AutodotsShapeRect
            {
                A = 39,
                B = 37,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[39].Value, GetMirrored(29), baseDots[37].Value }, ref shapePoints, true, true));
            //lip center
            Rects.Add(new AutodotsShapeRect
            {
                A = 15,
                B = 51,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[15].Value, ShapeDots[44], ShapeDots[45], baseDots[51].Value }, ref shapePoints));
            //temp
            dotsDictionary.Add(44, Rects.Last().ShapeIndices[1]);
            dotsDictionary.Add(45, Rects.Last().ShapeIndices[2]);
            //temp
            Rects.Add(new AutodotsShapeRect
            {
                A = 51,
                B = 37,
                Type = MeshPartType.Lip
            }.Initialise(new[] { baseDots[51].Value, GetMirrored(45), GetMirrored(44), baseDots[37].Value }, ref shapePoints, true));

            ShapeInfo.Points = new HeadPoints<HeadPoint>();
            ShapeInfo.Points.AddRange(shapePoints.Select(p => new HeadPoint(p)).ToArray());

            ShapeInfo.Indices = ShapeIndices.ToArray();
        }

        public List<int> InitializeProfile(List<Vector2> baseDots)
        {
            var result = new List<int>();
            ProfileRects.Clear();
            linkedPoints.Clear();
            var indices = new List<int>();
            var shapePoints = new List<Vector2>();
            var tmpLinks = new Dictionary<int, int>
                           {
                               {0, -1},//верх головы
                               {18, -1},//верх носа
                               {40, -1},
                               {19, -1},//нос
                               {41, -1},
                               {16, -1},//верх рта
                               {38, -1},
                               {51, -1},//цетр рта
                               {1, -1},//низ рта
                               {11, -1},//низ головы
                               {33, -1},
                           };
            foreach (var r in Rects)
            {
                if (tmpLinks.ContainsKey(r.A))
                    tmpLinks[r.A] = r.ShapeIndices[0];
                if (tmpLinks.ContainsKey(r.B))
                    tmpLinks[r.B] = r.ShapeIndices.Last();
            }
            if (tmpLinks.ContainsValue(-1))
                return result;
            linkedPoints.Add(0, new List<int> { tmpLinks[0] });
            linkedPoints.Add(1, new List<int> { tmpLinks[18], tmpLinks[40] });
            linkedPoints.Add(2, new List<int> { tmpLinks[19], tmpLinks[41] });
            linkedPoints.Add(3, new List<int> { tmpLinks[16], tmpLinks[38] });
            linkedPoints.Add(4, new List<int> { tmpLinks[51] });
            linkedPoints.Add(5, new List<int> { tmpLinks[1] });
            linkedPoints.Add(6, new List<int> { tmpLinks[11], tmpLinks[33] });

            //лоб
            var rect = new AutodotsShapeRect
            {
                A = 0,
                B = 1,
                Type = MeshPartType.ProfileTop
            }.Initialise(new[] { baseDots[0], new Vector2(0.0f, baseDots[1].Y) }, ref shapePoints);
            ProfileRects.Add(rect);
            var index1 = rect.ShapeIndices.Last();
            //profileDotsDictionary
            rect = new AutodotsShapeRect
            {
                A = 0,
                B = 1,
                Type = MeshPartType.ProfileTop,
                LinkedShapeRect = rect
            }.Initialise(new[] { baseDots[0], ShapeProfileDots[0], ShapeProfileDots[1], ShapeProfileDots[2],
                ShapeProfileDots[3], ShapeProfileDots[4], ShapeProfileDots[5], ShapeProfileDots[6], ShapeProfileDots[7], ShapeProfileDots[8],
                ShapeProfileDots[9], ShapeProfileDots[10], ShapeProfileDots[11], ShapeProfileDots[12], baseDots[1] }, ref shapePoints);
            ProfileRects.Add(rect);
            if (!result.Contains(rect.ShapeIndices.First()))
                result.Add(rect.ShapeIndices.First());
            if (!result.Contains(rect.ShapeIndices.Last()))
                result.Add(rect.ShapeIndices.Last());
            //temp
            profileDotsDictionary.Clear();
            profileDotsDictionary.Add(0, rect.ShapeIndices[1]);
            profileDotsDictionary.Add(1, rect.ShapeIndices[2]);
            profileDotsDictionary.Add(2, rect.ShapeIndices[3]);
            profileDotsDictionary.Add(3, rect.ShapeIndices[4]);
            profileDotsDictionary.Add(4, rect.ShapeIndices[5]);
            profileDotsDictionary.Add(5, rect.ShapeIndices[6]);
            profileDotsDictionary.Add(6, rect.ShapeIndices[7]);
            profileDotsDictionary.Add(7, rect.ShapeIndices[8]);
            profileDotsDictionary.Add(8, rect.ShapeIndices[9]);
            profileDotsDictionary.Add(9, rect.ShapeIndices[10]);
            profileDotsDictionary.Add(10, rect.ShapeIndices[11]);
            profileDotsDictionary.Add(11, rect.ShapeIndices[12]);
            profileDotsDictionary.Add(12, rect.ShapeIndices[13]);
            //temp

            for (int i = 0; i < rect.ShapeIndices.Length - 1; i++)
                indices.AddRange(new[] { index1, rect.ShapeIndices[i], rect.ShapeIndices[i + 1] });

            //верх носа
            rect = new AutodotsShapeRect
            {
                A = 1,
                B = 2,
                Type = MeshPartType.ProfileTop
            }.Initialise(new[] { new Vector2(0.0f, baseDots[1].Y), new Vector2(0.0f, baseDots[2].Y) }, ref shapePoints);
            ProfileRects.Add(rect);

            var index0 = rect.ShapeIndices[0];
            index1 = rect.ShapeIndices.Last();

            rect = new AutodotsShapeRect
            {
                A = 1,
                B = 2,
                Type = MeshPartType.ProfileTop,
                LinkedShapeRect = rect
            }.Initialise(new[] { baseDots[1], ShapeProfileDots[13], ShapeProfileDots[14], ShapeProfileDots[15],
                ShapeProfileDots[16], ShapeProfileDots[17], ShapeProfileDots[18], ShapeProfileDots[19], baseDots[2] }, ref shapePoints);
            ProfileRects.Add(rect);
            if (!result.Contains(rect.ShapeIndices.First()))
                result.Add(rect.ShapeIndices.First());
            if (!result.Contains(rect.ShapeIndices.Last()))
                result.Add(rect.ShapeIndices.Last());
            //temp
            profileDotsDictionary.Add(13, rect.ShapeIndices[1]);
            profileDotsDictionary.Add(14, rect.ShapeIndices[2]);
            profileDotsDictionary.Add(15, rect.ShapeIndices[3]);
            profileDotsDictionary.Add(16, rect.ShapeIndices[4]);
            profileDotsDictionary.Add(17, rect.ShapeIndices[5]);
            profileDotsDictionary.Add(18, rect.ShapeIndices[6]);
            profileDotsDictionary.Add(19, rect.ShapeIndices[7]);
            //temp

            indices.AddRange(new[] { index1, index0, rect.ShapeIndices[0] });
            for (int i = 0; i < rect.ShapeIndices.Length - 1; i++)
                indices.AddRange(new[] { index1, rect.ShapeIndices[i], rect.ShapeIndices[i + 1] });

            //низ носа
            rect = new AutodotsShapeRect
            {
                A = 2,
                B = 3,
                Type = MeshPartType.ProfileTop
            }.Initialise(new[] { new Vector2(0.0f, baseDots[2].Y), new Vector2(0.0f, baseDots[3].Y) }, ref shapePoints);
            ProfileRects.Add(rect);

            index0 = rect.ShapeIndices[0];
            index1 = rect.ShapeIndices.Last();

            rect = new AutodotsShapeRect
            {
                A = 2,
                B = 3,
                Type = MeshPartType.ProfileTop,
                LinkedShapeRect = rect
            }.Initialise(new[] { baseDots[2], ShapeProfileDots[20], ShapeProfileDots[21], ShapeProfileDots[22],
                ShapeProfileDots[23], ShapeProfileDots[24], baseDots[3] }, ref shapePoints, true);
            ProfileRects.Add(rect);
            if (!result.Contains(rect.ShapeIndices.First()))
                result.Add(rect.ShapeIndices.First());
            if (!result.Contains(rect.ShapeIndices.Last()))
                result.Add(rect.ShapeIndices.Last());
            //temp
            profileDotsDictionary.Add(20, rect.ShapeIndices[1]);
            profileDotsDictionary.Add(21, rect.ShapeIndices[2]);
            profileDotsDictionary.Add(22, rect.ShapeIndices[3]);
            profileDotsDictionary.Add(23, rect.ShapeIndices[4]);
            profileDotsDictionary.Add(24, rect.ShapeIndices[5]);
            //temp

            indices.AddRange(new[] { index1, index0, rect.ShapeIndices[0] });
            for (int i = 0; i < rect.ShapeIndices.Length - 1; i++)
                indices.AddRange(new[] { index1, rect.ShapeIndices[i], rect.ShapeIndices[i + 1] });

            //верх губы
            rect = new AutodotsShapeRect
            {
                A = 3,
                B = 4,
                Type = MeshPartType.ProfileBottom
            }.Initialise(new[] { new Vector2(0.0f, baseDots[3].Y), new Vector2(0.0f, baseDots[4].Y) }, ref shapePoints);
            ProfileRects.Add(rect);

            index0 = rect.ShapeIndices[0];
            index1 = rect.ShapeIndices.Last();

            rect = new AutodotsShapeRect
            {
                A = 3,
                B = 4,
                Type = MeshPartType.ProfileBottom,
                LinkedShapeRect = rect
            }.Initialise(new[] { baseDots[3], ShapeProfileDots[25], ShapeProfileDots[26], ShapeProfileDots[27], baseDots[4] }, ref shapePoints);
            ProfileRects.Add(rect);
            if (!result.Contains(rect.ShapeIndices.First()))
                result.Add(rect.ShapeIndices.First());
            if (!result.Contains(rect.ShapeIndices.Last()))
                result.Add(rect.ShapeIndices.Last());
            //temp
            profileDotsDictionary.Add(25, rect.ShapeIndices[1]);
            profileDotsDictionary.Add(26, rect.ShapeIndices[2]);
            profileDotsDictionary.Add(27, rect.ShapeIndices[3]);
            //temp

            indices.AddRange(new[] { index1, index0, rect.ShapeIndices[0] });
            for (int i = 0; i < rect.ShapeIndices.Length - 1; i++)
                indices.AddRange(new[] { index1, rect.ShapeIndices[i], rect.ShapeIndices[i + 1] });

            //низ губы
            rect = new AutodotsShapeRect
            {
                A = 4,
                B = 5,
                Type = MeshPartType.ProfileBottom
            }.Initialise(new[] { new Vector2(0.0f, baseDots[4].Y), new Vector2(0.0f, baseDots[5].Y) }, ref shapePoints);
            ProfileRects.Add(rect);

            index0 = rect.ShapeIndices[0];
            index1 = rect.ShapeIndices.Last();

            rect = new AutodotsShapeRect
            {
                A = 4,
                B = 5,
                Type = MeshPartType.ProfileBottom,
                LinkedShapeRect = rect
            }.Initialise(new[] { baseDots[4], ShapeProfileDots[28], ShapeProfileDots[29], ShapeProfileDots[30], baseDots[5] }, ref shapePoints);
            ProfileRects.Add(rect);
            if (!result.Contains(rect.ShapeIndices.First()))
                result.Add(rect.ShapeIndices.First());
            if (!result.Contains(rect.ShapeIndices.Last()))
                result.Add(rect.ShapeIndices.Last());
            //temp
            profileDotsDictionary.Add(28, rect.ShapeIndices[1]);
            profileDotsDictionary.Add(29, rect.ShapeIndices[2]);
            profileDotsDictionary.Add(30, rect.ShapeIndices[3]);
            //temp

            indices.AddRange(new[] { index1, index0, rect.ShapeIndices[0] });
            for (int i = 0; i < rect.ShapeIndices.Length - 1; i++)
                indices.AddRange(new[] { index1, rect.ShapeIndices[i], rect.ShapeIndices[i + 1] });

            //подбородок
            rect = new AutodotsShapeRect
            {
                A = 5,
                B = 6,
                Type = MeshPartType.ProfileBottom
            }.Initialise(new[] { new Vector2(0.0f, baseDots[5].Y), baseDots[6] }, ref shapePoints);
            ProfileRects.Add(rect);

            index0 = rect.ShapeIndices[0];
            index1 = rect.ShapeIndices.Last();

            rect = new AutodotsShapeRect
            {
                A = 5,
                B = 6,
                Type = MeshPartType.ProfileBottom,
                LinkedShapeRect = rect
            }.Initialise(new[] { baseDots[5], ShapeProfileDots[31], ShapeProfileDots[32], ShapeProfileDots[33],
                ShapeProfileDots[34], ShapeProfileDots[35], ShapeProfileDots[36], ShapeProfileDots[37], ShapeProfileDots[38], 
                ShapeProfileDots[39], ShapeProfileDots[40], ShapeProfileDots[41], ShapeProfileDots[42], baseDots[6] }, ref shapePoints, true);
            ProfileRects.Add(rect);
            if (!result.Contains(rect.ShapeIndices.First()))
                result.Add(rect.ShapeIndices.First());
            if (!result.Contains(rect.ShapeIndices.Last()))
                result.Add(rect.ShapeIndices.Last());
            //temp
            profileDotsDictionary.Add(31, rect.ShapeIndices[1]);
            profileDotsDictionary.Add(32, rect.ShapeIndices[2]);
            profileDotsDictionary.Add(33, rect.ShapeIndices[3]);
            profileDotsDictionary.Add(34, rect.ShapeIndices[4]);
            profileDotsDictionary.Add(35, rect.ShapeIndices[5]);
            profileDotsDictionary.Add(36, rect.ShapeIndices[6]);
            profileDotsDictionary.Add(37, rect.ShapeIndices[7]);
            profileDotsDictionary.Add(38, rect.ShapeIndices[8]);
            profileDotsDictionary.Add(39, rect.ShapeIndices[9]);
            profileDotsDictionary.Add(40, rect.ShapeIndices[10]);
            profileDotsDictionary.Add(41, rect.ShapeIndices[11]);
            profileDotsDictionary.Add(42, rect.ShapeIndices[12]);
            //temp

            for (int i = 0; i < rect.ShapeIndices.Length - 3; i++)
                indices.AddRange(new[] { index0, rect.ShapeIndices[i], rect.ShapeIndices[i + 1] });

            indices.AddRange(new[] { index0, rect.ShapeIndices[rect.ShapeIndices.Length - 3], index1 });
            indices.AddRange(new[] { index1, rect.ShapeIndices[rect.ShapeIndices.Length - 3], rect.ShapeIndices[rect.ShapeIndices.Length - 2] });

            ShapeProfileInfo.Points = new HeadPoints<HeadPoint>();
            ShapeProfileInfo.Points.AddRange(shapePoints.Select(p => new HeadPoint(p)).ToArray());

            ShapeProfileInfo.Indices = indices.ToArray();
            return result;
        }

        public List<Vector2> ProfileDots
        {
            get
            {
                var result = new Vector2[ShapeProfileDots.Count];
                foreach (var d in profileDotsDictionary)
                {
                    result[d.Key] = ShapeProfileInfo.Points[d.Value].Value;
                }
                return result.ToList();
            }
        }

        public List<Vector2> Dots
        {
            get
            {
                var result = new Vector2[ShapeDots.Count];
                foreach (var d in dotsDictionary)
                {
                    result[d.Key] = ShapeInfo.Points[d.Value].Value;
                }
                return result.ToList();
            }
        }

        public List<HeadPoint> GetBaseDots()
        {
            var pointsDict = new Dictionary<int, HeadPoint>();
            foreach (var rect in Rects)
            {
                if (!pointsDict.ContainsKey(rect.A))
                    pointsDict.Add(rect.A, new HeadPoint(rect.Points[0]) { Visible = rect.Type != MeshPartType.REar && rect.Type != MeshPartType.LEar });
                if (!pointsDict.ContainsKey(rect.B))
                    pointsDict.Add(rect.B, new HeadPoint(rect.Points.Last()) { Visible = rect.Type != MeshPartType.REar && rect.Type != MeshPartType.LEar });
            }
            var result = new HeadPoint[defaultDots.Count];
            foreach (var p in pointsDict)
                result[p.Key] = p.Value;
            for(int i = 0; i<result.Length; i++)
                if (result[i] == null)
                    result[i] = defaultDots[i];

            result[7].LinkedPoints.Add(47);      // уши
            result[8].LinkedPoints.Add(48);
            result[29].LinkedPoints.Add(49);
            result[30].LinkedPoints.Add(50);

            return result.ToList();
        }

        public List<Vector2> GetProfileBaseDots()
        {
            var result = new Vector2[7];
            foreach (var rect in ProfileRects)
            {
                Vector2 a = rect.Points[0], b = rect.Points.Last();
                var list = linkedPoints[rect.A];
                a.Y = 0.0f;
                foreach (var l in list)
                    a.Y += ShapeInfo.Points[l].Value.Y;
                a.Y /= list.Count;
                result[rect.A] = a;
                list = linkedPoints[rect.B];
                b.Y = 0.0f;
                foreach (var l in list)
                    b.Y += ShapeInfo.Points[l].Value.Y;
                b.Y /= list.Count;
                result[rect.B] = a;
            }
            return result.ToList();
        }

        public void TransformRects()
        {
            foreach (var rect in Rects)
                for (var i = 0; i < rect.ShapeIndices.Length; i++)
                {
                    rect.Points[i] = ShapeInfo.Points[rect.ShapeIndices[i]].Value;
                    rect.Points = rect.Points;
                }
            foreach (var rect in ProfileRects.Where(r => r.LinkedShapeRect != null))
                for (var i = 0; i < rect.ShapeIndices.Length; i++)
                {
                    rect.Points[i] = ShapeProfileInfo.Points[rect.ShapeIndices[i]].Value;
                    rect.Points = rect.Points;
                }
            UpdateProfileLines();
        }

        public void InitializeShaping()
        {
            headMeshesController.InitializeShaping(ref ShapeInfo);
            headMeshesController.InitializeShapingProfile(ref ShapeProfileInfo);
        }
    }
}
