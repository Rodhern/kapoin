#include "colors.inc"    // The include files contain
#include "textures.inc"
#include "stones.inc"    // pre-defined scene elements
#include "woods.inc"
#include "metals.inc"


sphere {
  <3.80, -1.50, 3.00>, 2.8
  texture {
    pigment { color rgb <0.05,0.15,0.95> }
    normal { bumps 0.12 scale 0.02 }
    finish { phong 0.3 }
    }
  }

sphere {
  <3.61, -1.4175, 2.65>, 2.56
  texture {
    pigment { color Green }
    normal { bumps 0.2 scale 0.08 }
    finish { phong 0.1 }
    }
  }

torus {
  4.225, 0.025
  rotate +35*x +90*y +0*z
  translate <3.80, -1.50, 3.00>
  pigment { color rgb <0.8,0.8,0.6> }
  finish { reflection 0.6 metallic
    irid { 0.35
           thickness .5
           turbulence .5 } }
  }

#declare C0 =  <0, 0, -0.5>;
#declare T1 = <0, 2, 0>;
#declare T2 = <-1.9021,  0.6180, 0>;
#declare T3 = <-1.1756, -1.6180, 0>;
#declare T4 = < 1.1756, -1.6180, 0>;
#declare T5 = < 1.9021,  0.6180, 0>;
#declare B12 = <-0.41145,  0.56631, 0>;
#declare B23 = <-0.66574, -0.21631, 0>;
#declare B34 = <0, -0.7, 0>;
#declare B45 = < 0.66574, -0.21631, 0>;
#declare B51 = < 0.41145,  0.56631, 0>;
#declare S1 =  0.14; // scale of star
#declare R1 =  -10*x -10*y +6*z; // tilt of star
#declare C1 =  <0.8719, 0.5503, 0.7664>; // new center of star

triangle { C0, T1, B12
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T1, B51
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T2, B23
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T2, B12
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T3, B34
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T3, B23
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T4, B45
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T4, B34
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T5, B51
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }
triangle { C0, T5, B45
  texture { T_Gold_2A }
  scale S1 rotate R1 translate C1
  }


plane {
  z,25
  texture {
    pigment { color rgb <0.85,0,0> }
    finish { reflection 0.15 specular 0.05 }
    normal { bumps 0.2 scale 0.08 }
    }
  }

box {
  <-50, 5, 24.9>, < 50, 9, 25.1>
  rotate 22*z
  texture {
    pigment { color rgb <0.90,0.95,0.95> }
    finish { reflection 0.15 specular 0.05 }
    normal { bumps 0.2 scale 0.08 }
    }
  }


text {
  ttf "timrom.ttf" "Kerbal Space Center" 0.08, 0
  //pigment { Black }
  texture { T_Stone10 }
  finish { reflection rgb <0.05,0.05,0.1> ambient 0.05 diffuse 0.1 }
  scale 1.2
  rotate +2*y
  translate -5.5*x +2.45*y +4*z
  }

box {
  <-6, 2.05, 3.8>, < 5, 2.10, 4.1>
  rotate 2*y
  texture {
    T_Stone10
    finish { reflection rgb <0.05,0.05,0.1> ambient 0.05 diffuse 0.1 }
    //finish { reflection 0.15 specular 0.05 }
    //normal { bumps 0.2 scale 0.08 }
    }
  }

box {
  <-6, 3.55, 3.8>, < 5, 3.60, 4.1>
  rotate 2*y
  texture {
    T_Stone10
    finish { reflection rgb <0.05,0.05,0.1> ambient 0.05 diffuse 0.1 }
    //finish { reflection 0.15 specular 0.05 }
    //normal { bumps 0.2 scale 0.08 }
    }
  }

text {
  ttf "timrom.ttf" "Kapoin" 0.12, 0
  texture { Polished_Chrome }
  scale 1.0
  rotate -1*y -0.6*z
  translate -5.85*x -3.45*y +4*z
  }


light_source {
  <2, 4, 6> color rgb <1,0.5,0.5>
  }

light_source {
  <2.5, 3.5, -3> color White shadowless
  }

camera {
  location <0, 0.15, -4>
  look_at  <0, 0.10,  2>
  right <16,0,0>/10 up <0,10,0>/10 // FoV aspect ratio and zoom values
  }
