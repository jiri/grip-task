using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour {
    [Header("Context")]
    public FPSController player;
    public BlockAtlas atlas;

    [Header("Content")]
    public byte[] blocks;

    [Header("Utility")]
    public RectTransform highlight;
    public Image[] slotIcons;

    int currentSlot = 0;
    float scroll = 0.0f;
    float scrollSensitivity = 0.2f;

    void OnValidate() {
        for (int i = 0; i < this.blocks.Length; i++) {
            this.blocks[i] = (byte)Mathf.Clamp(this.blocks[i], 0, this.atlas.prototypes.Length);
        }
    }

    void Start() {
        this.currentSlot = this.player.selectedBlock;

        for (int i = 0; i < this.blocks.Length; i++) {
            this.slotIcons[i].sprite = this.atlas.prototypes[this.blocks[i]].icon;
        }
    }

    void Update() {
        this.scroll += Input.GetAxis("Mouse ScrollWheel");

        if (this.scroll > this.scrollSensitivity) {
            this.currentSlot += 1;
            this.scroll -= this.scrollSensitivity;
        }
        else if (this.scroll < -this.scrollSensitivity) {
            this.currentSlot -= 1;
            this.scroll += this.scrollSensitivity;
        }

        while (this.currentSlot > 4) {
            this.currentSlot -= 5;
        }
        while (this.currentSlot < 0) {
            this.currentSlot += 5;
        }

        this.highlight.position = this.slotIcons[currentSlot].transform.position;
        this.player.selectedBlock = this.blocks[this.currentSlot];
    }
}
