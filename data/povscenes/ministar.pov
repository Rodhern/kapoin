#include "colors.inc"    // The include files contain
#include "textures.inc"
#include "stones.inc"    // pre-defined scene elements
#include "metals.inc"


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

triangle { C0, T1, B12
  texture { T_Gold_2A }
  }
triangle { C0, T1, B51
  texture { T_Gold_2A }
  }
triangle { C0, T2, B23
  texture { T_Gold_2A }
  }
triangle { C0, T2, B12
  texture { T_Gold_2A }
  }
triangle { C0, T3, B34
  texture { T_Gold_2A }
  }
triangle { C0, T3, B23
  texture { T_Gold_2A }
  }
triangle { C0, T4, B45
  texture { T_Gold_2A }
  }
triangle { C0, T4, B34
  texture { T_Gold_2A }
  }
triangle { C0, T5, B51
  texture { T_Gold_2A }
  }
triangle { C0, T5, B45
  texture { T_Gold_2A }
  }


light_source {
  <2, 4, -3> color White
  }

camera {
  location <0, 0.15, -4>
  look_at  <0, 0.10,  2>
  right 1/1 // inverse horizontal scale factor, assuming a default 4:3 scene camera setting.
  }
