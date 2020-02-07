﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar : MonoBehaviour {
    public World world;
    public FPSController player;

    public RectTransform highlight;
    public RectTransform[] icons;

    int slotIndex = 0;
    float scroll = 0.0f;
    float scrollSensitivity = 0.2f;

    void Start() {
        this.slotIndex = this.player.selectedBlock;
    }

    void Update() {
        this.scroll += Input.GetAxis("Mouse ScrollWheel");

        if (this.scroll > this.scrollSensitivity) {
            this.slotIndex += 1;
            this.scroll -= this.scrollSensitivity;
        }
        else if (this.scroll < -this.scrollSensitivity) {
            this.slotIndex -= 1;
            this.scroll += this.scrollSensitivity;
        }

        while (this.slotIndex > 4) {
            this.slotIndex -= 5;
        }
        while (this.slotIndex < 0) {
            this.slotIndex += 5;
        }

        this.highlight.position = this.icons[slotIndex].position;
        this.player.selectedBlock = (byte)this.slotIndex;
    }
}