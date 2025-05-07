using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdBannerController : MonoBehaviour
{
    private void OnEnable()
    {
        AdsManager.ShowBannerAd();
    }
    
    private void OnDisable()
    {
        AdsManager.HideBannerAd();
    }

    private void OnDestroy()
    {
        AdsManager.HideBannerAd();
    }
}
