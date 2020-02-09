using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour {
    public FPSController player;

    float width;

    GameObject background;
    GameObject foreground;
    RectTransform fgTransform;

    void Start() {
        this.width = GetComponent<RectTransform>().sizeDelta.x;

        this.background = this.transform.Find("Background").gameObject;
        this.foreground = this.transform.Find("Foreground").gameObject;

        this.fgTransform = this.foreground.GetComponent<RectTransform>();
    }

    void Update() {
        this.background.SetActive(this.player.isBreaking);
        this.foreground.SetActive(this.player.isBreaking);

        this.fgTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.player.breakProgress * this.width);
    }
}
