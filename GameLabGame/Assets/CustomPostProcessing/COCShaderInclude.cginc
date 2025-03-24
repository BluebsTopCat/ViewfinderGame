//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void COCShaderInclude_float(float Aperture, float PlaneInFocus, float ObjectDistance, float ImageDistance, out float CoC)
{
    const float f = (PlaneInFocus * ImageDistance)/(PlaneInFocus + ImageDistance);
    CoC = Aperture * ((f *(PlaneInFocus - ObjectDistance))/(ImageDistance * (PlaneInFocus - f)));
}
#endif //MYHLSLINCLUDE_INCLUDED