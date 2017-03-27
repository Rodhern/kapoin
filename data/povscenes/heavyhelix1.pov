#include "colors.inc"    // The include files contain
#include "textures.inc"
#include "woods.inc"     // pre-defined scene elements
#include "stones.inc"
#include "metals.inc"


#declare sqrthalf = 0.70710678;
#declare dPO = 0.35;
#declare dCC = 0.40;
#declare linkvec = < -0.80*dCC, dCC*2.8+dPO*2, 1.6*sqrthalf*dCC >;

#declare Pball =
  sphere {
    <0, 0, 0>, 0.30
    texture {
      pigment { color Yellow transmit 0.2}
      normal { bumps 0.1 scale 0.1 }
      finish { reflection {0.4 metallic} phong 0.2 }
      }
    }

#declare Nball =
  union {
    sphere {
      <0, 0, 0>, 0.24
      texture {
        pigment { color Blue transmit 0.2}
        normal { bumps 0.1 scale 0.1 }
        finish { reflection {0.4 metallic} phong 0.2 }
        }
      }
    cylinder {
      <0, 0, 0>, <0.32, 0, 0>, 0.06
        pigment { color rgb <0.2,0.2,1> transmit 0.3}
        normal { bumps 0.1 scale 0.1 }
        finish { reflection {0.4 metallic} phong 0.2 }
      }
    cylinder {
      <0, 0, 0>, <0.32, 0, 0>, 0.06
        rotate 120*y
        pigment { color rgb <0.2,0.2,1> transmit 0.3}
        normal { bumps 0.1 scale 0.1 }
        finish { reflection {0.4 metallic} phong 0.2 }
      }
    }

#declare Cball =
  sphere {
    <0, 0, 0>, 0.25
    texture {
      pigment { color rgb <0.35, 0.35, 0.35> transmit 0.2}
      normal { bumps 0.1 scale 0.1 }
      finish { reflection {0.4 metallic} phong 0.2 }
      }
    }

#declare Oball =
  sphere {
    <0, 0, 0>, 0.20
    texture {
      pigment { color Red transmit 0.2}
      normal { bumps 0.1 scale 0.1 }
      finish { reflection {0.4 metallic} phong 0.2 }
      }
    }

#declare Hball =
  sphere {
    <0, 0, 0>, 0.15
    texture {
      pigment { color rgb <0.9, 0.9, 0.9> transmit 0.2}
      normal { bumps 0.1 scale 0.1 }
      finish { reflection {0.4 metallic} phong 0.2 }
      }
    }


#declare Pgroup =
  union {
    object { Pball }
    object { Oball translate dPO*< +sqrthalf, -1,  0> }
    object { Oball translate dPO*< +sqrthalf, +1,  0> }
    object { Oball translate dPO*< -sqrthalf,  0, +1> }
    object { Oball translate dPO*< -sqrthalf,  0, -1> }
    object { Hball translate dPO*< -sqrthalf,  0, -1>
                   translate dPO*< 0.27, 0, -0.50> }
    translate -dPO*< +sqrthalf, -1,  0> // new pivot point is the red connector         
    }

#declare Cchain =
  union {
    object { Cball } // C_2
    object { Cball translate dCC*<  0, -1, +sqrthalf> } // C_1
    object { Cball translate dCC*<  0, +1, +sqrthalf> } // C_3
    object { Hball translate dCC*< -1,  0, -sqrthalf> * 0.5 }
    object { Oball translate dCC*<1+sqrthalf, -1, 2*sqrthalf-1 > * 0.5 } // forced to midpoint
    object { Cball translate dCC*<  0, -1, +sqrthalf>
                   translate dCC*<  1,  0, +sqrthalf> } // C_14
    object { Cball translate dCC*<  sqrthalf, 0, -1> // end translation (forced twist)
                   translate dCC*<  0, -1, +sqrthalf>
                   translate dCC*<  1,  0, +sqrthalf> } // C_25
    object { Hball translate dCC*<1+sqrthalf, -1, 2*sqrthalf-1>
                   translate dCC*< sqrthalf, 0, 1> * 0.5 }
    object { Nball translate dCC*<1+sqrthalf, -1, 2*sqrthalf-1>
                   translate dCC*< sqrthalf, 0, -1> * 0.9 }
    object { Hball translate dCC*<  0, -1, +sqrthalf> // for C_1
                   translate dCC*< -1,  0, +sqrthalf> * 0.5 }
    object { Hball translate dCC*<  1, -1, 2*sqrthalf>
                   translate dCC*<  0, 1, sqrthalf > * 0.5 } // for C_14 (1)
    object { Hball translate dCC*<  1, -1, 2*sqrthalf>
                   translate dCC*<  0, -1, sqrthalf > * 0.5 } // for C_14 (2)
    object { Hball translate dCC*<  0, +1, +sqrthalf> // for C_3 (1)
                   translate dCC*<  0, +1, -sqrthalf> * 0.5 }
    object { Hball translate dCC*<  0, +1, +sqrthalf> // for C_3 (2)
                   translate dCC*< +1,  0, +sqrthalf> * 0.5 }
    //object { Oball translate dCC*<  0, +1, +sqrthalf> // C_3 connection node illustration
    //               translate dCC*< -1,  0, +sqrthalf> * 0.8 }
    translate dPO*< +sqrthalf, +1,  0> // upper red (O)
    translate dCC*< 0, 1, -sqrthalf> // lower grey (C)
    translate dCC*< 0, 1, sqrthalf> * 0.8 // connection
    translate -dPO*< +sqrthalf, -1,  0> // move to match up with Pgroup above
  }


#declare zoffdeg = -9.9721;
#declare xoffdeg = -13.761;
#declare Thetas = array[60]
  { 440.6225, 406.2451, 371.8676, 337.4901, 303.1127,
    268.7352, 234.3577, 199.9803, 165.6028, 131.2253,
     96.8479,  62.4704,  28.0929,  -6.2845, -40.6620,
    -75.0395, -109.4170, -143.7944, -178.1719, -212.5494,
   -246.9268, -281.3043, -315.6818, -350.0592, -384.4367,
   -418.8142, -453.1916, -487.5691, -521.9466, -556.3240,
    290.623, 256.245,  221.868,  187.490,  153.113,  118.735,  84.358,  49.980,  15.603, -18.775,
    -53.152, -87.530,  -121.907, -156.285, -190.662, -225.039,-259.417,-293.794,-328.172,-362.549,
   -396.927,-431.304,  -465.682, -500.059, -534.437, -568.814,-603.192,-637.569,-671.947,-706.324 }
#declare rotzs = array[60]
  { -52.9116, -59.3040,  -55.5492,  -35.8184,   14.9162,  49.2711,   58.7930, 57.3959,   42.9208,  -1.6264,
    -44.3277,  -57.7351,  -58.5934,  -48.2282, -11.8688,  37.6909,  56.0531,  59.2296,  52.1474,  23.9659,
    -28.9427, -53.6196,  -59.3496,  -55.0010,  -33.8209,  17.8642,  50.2409,   58.9619, 57.0204,  41.4128,
     31.8287,   54.4440,   59.3666,   54.2359,   31.0932,  -21.4491,  -51.3760,  -59.1335,  -56.4877,  -39.3392,
      8.9329,   47.1786,   58.3775,   58.0215,   45.5613,    4.6685,  -41.5085,  -57.0446,  -58.9524,  -50.1833,
    -17.6861,   33.9474,   55.0360,   59.3477,   53.5777,   28.7977,  -24.1268,  -52.1961,  -59.2350,  -56.0234 }
#declare rotxs = array[60]
  {  -32.3347,   -3.4895,   25.7467,   51.0694,   58.1764,  38.6549,   10.4501,  -18.9795,  -45.9083,  -59.3533,
     -44.5788,  -17.3532,   12.0964,   40.1036,   58.6229,  49.9162,   24.1530,   -5.1458,  -33.8640,  -56.1093,
     -54.3904,  -30.7904,   -1.8322,   27.3305,   52.1698,  57.6325,   37.1840,    8.8006,  -20.5994,  -47.2020,
      53.15032,   28.80681,   -0.27632,  -29.32866,  -53.48554,  -56.80761,  -35.28445,  -6.698810,  22.64901,   48.78990,
      58.94951,   41.44133,   13.63810,  -15.82165,  -43.30120,  -59.25399,  -47.12354,  -20.49998,   8.90205,   37.27502,
      57.66864,   52.10381,   27.23341,   -1.93412,  -30.88572,  -54.44763,  -56.06104,  -33.77044,  -5.04398,   24.25127 }
#declare Cs = array[60]
  { <  2.285960,    0.969407,    1.563910>
    <  1.003635,    1.938814,    2.581500>
    < -0.629289,    2.908221,    2.697299>
    < -2.042385,    3.877628,    1.870853>
    < -2.742016,    4.847035,    0.390865>
    < -2.483782,    5.816442,   -1.225664>
    < -1.357892,    6.785849,   -2.414033>
    <  0.242349,    7.755256,   -2.759111>
    <  1.757931,    8.724662,   -2.140352>
    <  2.659416,    9.694069,   -0.773907>
    <  2.631892,   10.663476,    0.862887>
    <  1.684971,   11.632883,    2.198249>
    <  0.149442,   12.602290,    2.765700>
    < -1.438292,   13.571697,    2.367012>
    < -2.523589,   14.541104,    1.141459>
    < -2.727324,   15.510511,   -0.482839>
    < -1.978326,   16.479918,   -1.938467>
    < -0.538242,   17.449325,   -2.716933>
    <  1.089865,   18.418732,   -2.546296>
    <  2.337251,   19.388139,   -1.486164>
    <  2.768168,   20.357546,    0.093127>
    <  2.232084,   21.326953,    1.639886>
    <  0.916269,   22.296360,    2.613786>
    < -0.719625,   23.265767,    2.674615>
    < -2.104134,   24.235174,    1.801124>
    < -2.753608,   25.204580,    0.298449>
    < -2.441167,   26.173987,   -1.308483>
    < -1.275957,   27.143394,   -2.458325>
    <  0.334982,   28.112801,   -2.749403>
    < 1.8289020,   29.082208,   -2.080035>
    < -2.761654,    0.969407,   -0.211405> // edge two
    < -2.159923,    1.938814,   -1.733828>
    < -0.803669,    2.908221,   -2.650574>
    <  0.833330,    3.877628,   -2.641399>
    <  2.179223,    4.847035,   -1.709507>
    <  2.763851,    5.816442,   -0.180435>
    <  2.382986,    6.785849,    1.411668>
    <  1.169675,    7.755256,    2.510635>
    < -0.452236,    8.724662,    2.732565>
    < -1.916169,    9.694069,    1.999931>
    < -2.710728,   10.663476,    0.568664>
    < -2.558353,   11.632883,   -1.061254>
    < -1.512270,   12.602290,   -2.320445>
    <  0.062091,   13.571697,   -2.769038>
    <  1.614762,   14.541104,   -2.250326>
    <  2.603351,   15.510511,   -0.945511>
    <  2.682514,   16.479918,    0.689599>
    <  1.824598,   17.449325,    2.083812>
    <  0.329297,   18.418732,    2.750089>
    < -1.281037,   19.388139,    2.455682>
    < -2.443867,   20.357546,    1.303434>
    < -2.752985,   21.326953,   -0.304141>
    < -2.100405,   22.296360,   -1.805471>
    < -0.714094,   23.265767,   -2.676097>
    <  0.921671,   24.235174,   -2.611886>
    <  2.235470,   25.204580,   -1.635268>
    <  2.768355,   26.173987,   -0.087404>
    <  2.334174,   27.143394,    1.490993>
    <  1.084599,   28.112801,    2.548543>
    < -0.543858,   29.082208,    2.715814> }

#declare FaintGreen = rgb <0,0.20,0.12>;
#declare helixvec = 29.08221*y; // the length of "HelixEdge"
#declare helixrot = 48.676*y; // the residual twist of "HelixEdge"


#declare HelixEdge =
  union {
  #declare idx = 59;
  #while (idx >= 0)
  
  union {
    object { Pgroup }
    object { Cchain }
    rotate zoffdeg*z
    rotate xoffdeg*x
    rotate Thetas[idx]*y
    scale 1.0 // used for debug (by making the objects smaller)
    rotate -rotxs[idx]*x
    rotate -rotzs[idx]*z
    translate Cs[idx]
    }
  
  #declare idx = idx - 1;
  #end }

#declare HelixBars =
  union {
  #declare idx = 29;
  #while (idx >= 0)
  
  #declare BE1 =
    vrotate(vrotate(vrotate(vrotate(vrotate(
    <0, 0, 0>
    + dCC*<1+sqrthalf, -1, 2*sqrthalf-1>
    + dCC*< sqrthalf, 0, -1> * 0.9
    + dPO*< +sqrthalf, +1,  0>
    + dCC*< 0, 1, -sqrthalf>
    + dCC*< 0, 1, sqrthalf> * 0.8
    +-dPO*< +sqrthalf, -1,  0>,
      zoffdeg*z),
      xoffdeg*x),
      Thetas[idx]*y),
      -rotxs[idx]*x),
      -rotzs[idx]*z)
    + Cs[idx];
  
  #declare BE2 =
    vrotate(vrotate(vrotate(vrotate(vrotate(
    <0, 0, 0>
    + dCC*<1+sqrthalf, -1, 2*sqrthalf-1>
    + dCC*< sqrthalf, 0, -1> * 0.9
    + dPO*< +sqrthalf, +1,  0>
    + dCC*< 0, 1, -sqrthalf>
    + dCC*< 0, 1, sqrthalf> * 0.8
    +-dPO*< +sqrthalf, -1,  0>,
      zoffdeg*z),
      xoffdeg*x),
      Thetas[idx+30]*y),
      -rotxs[idx+30]*x),
      -rotzs[idx+30]*z)
    + Cs[idx+30];
  
  #declare ConnectBar =
    cylinder {
      (0.15*BE1 + 0.85*BE2), (0.85*BE1 + 0.15*BE2), 0.08 open
      texture {
        pigment { color FaintGreen transmit 0.4}
        normal { bumps 0.1 scale 0.1 }
        finish { reflection {0.2} phong 0.1 }
      }
    }
  
  object { ConnectBar }
  
  #declare idx = idx - 1;
  #end }

            
union {
  union {
    object { HelixEdge }
    object { HelixBars }
    rotate helixrot    * 3
    rotate -5*z // 'random' noise
    translate helixvec
    rotate 4*x + -5*z // 'random' noise
    translate helixvec * 2 }
  union {
    object { HelixEdge }
    object { HelixBars }
    rotate helixrot    * 2
    rotate 4*x + -5*z // 'random' noise
    translate helixvec * 2 } // if needed for background copy addtional pieces this way
  union {
    object { HelixEdge }
    object { HelixBars }
    rotate helixrot    * 1
    translate helixvec * 1 }
  object { HelixEdge }
  object { HelixBars }
  rotate 40*z
  rotate 10*x
  translate <10,-10,190> }

union {
  union {
    object { HelixEdge }
    object { HelixBars }
    rotate helixrot    * 1
    translate helixvec * 1 }
  object { HelixEdge }
  object { HelixBars }
  rotate 60*z
  translate <25,-20,40> }

union {
  object { HelixEdge }
  //object { HelixBars } // the bars are more of a distance effect
  rotate 100*z
  rotate -65*y
  rotate -10*x
  translate <12,6,18> }
  


light_source {
  <2, 4, -3> color White
  fade_distance 10
  fade_power 1.5
  }           
light_source {
  <-8, -2, -6> color White
  fade_distance 10
  fade_power 1.5
  }  

camera {
  location <0.01, 0.02, -10>
  look_at  <0, 0, 0>
  right 1*x up 10/16*y
  focal_point <0, 0, 20>
  aperture 1.0
  }
