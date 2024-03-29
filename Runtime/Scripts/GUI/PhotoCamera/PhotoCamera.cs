using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PhotoCamera
{
    public void Show(bool useFrontCameraOnMobile, bool closeAfterTakingPhoto, Action closeAction, Action<Texture2D, Utils.GyroInfo> tookPhotoAction);
    public void Hide();
}
