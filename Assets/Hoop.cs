using System.Collections.Generic;
using UnityEngine;



public class HoopNode {
    int Dist; // ...ance from root Hoop, or previous HoopNode 
    int Ang;
}



public class Hoop {
    bool Snake; // not looped, not closed (not an orobourus) 
    Vector3 Pos; // of the 1st point 
    int InitDistRes; // distance resolution 
    int DistDoublings; //   of resolution 
    int InitAngRes; // angular resolution 
    int AngDoublings; //    of resolution 
    List<HoopNode>Nodes = new List<HoopNode>();
}






public enum EdMode { // DRAW mode? 
    LineSegment,
    Snake, /* or hoop */, // or linked lines, line chain 
}
public enum ActType {
    PreAct, // previous/old 
    CurAct, // current/new 
}
public struct Act {
    ActType Type;
    Vector3 Pos;
}



public class FadeEd {
    public EdMode Ed;

    // cursor 
    int CURSOR = 0; // 1st id in Spheres (ACTUALLY THIS SHOULD JUST BE A SEPARATE GAMEOBJECT?) 
    Color CursCol;
    // misc 
    List<GameObject>Spheres  = new List<GameObject>();
    List<Act>       HistActs = new List<Act>(); // histories of ___ 
    List<Act>       HistOps  = new List<Act>();



    public FadeEd () {
        Spheres.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
        var c = Spheres[CURSOR].GetComponent<Collider>();
        GameObject.DestroyImmediate(c);
    }


    public void Update (Vector3 cursor) {
        Spheres[CURSOR].transform.position = cursor;

        // cycle color channel values 
        var cc = Time.time % 1.0f;
        CursCol = new Color(cc, cc, cc);
    }
}
