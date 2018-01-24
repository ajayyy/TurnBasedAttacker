using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupData {
    public Vector2 position = Vector2.zero;

    public int type = -1;

    public PickupData(Vector2 position, int type) {
        this.position = position;
        this.type = type;
    }
}
