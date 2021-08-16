using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class App : MonoBehaviour {
    const int mid = 2;
    const float lWallEdge = -2f;
    const float rWallEdge =  2f;
    const float wallWid = rWallEdge - lWallEdge;
    GameObject cubeGO;
    GameObject planeGO;
    List<GameObject> gos = new List<GameObject>();



    void Start () {
        cubeGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeGO.transform.position = Vector3.up * 3f;
        
        planeGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeGO.SetActive(false);

        for (int i = 0; i < 5; i++) {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            var x = 0f;
            var y = Random.Range(3f, 10f);

            if (i < 2)
                x = lWallEdge; // make lefts 
            if (i > 2)
                x = rWallEdge; // make rights 

            // (b)lue == (b)ottom for each side (what SHOULD be on bottom (if it matters)?) 
            if (i == 0 || i == 4)
                go.GetComponent<Renderer>().material.color = Color.blue;

            if (i == mid) {
                go.GetComponent<Renderer>().material.color = Color.magenta;
                var f = 0.2f;
                go.transform.localScale = new Vector3(f, f, f);
            }

            go.transform.position = new Vector3(x, y, 0);
            gos.Add(go);
        }
    }



    class ClickInfo {
        public RaycastHit Hit; // picked object being resized 
        public Plane Plane;
        public Vector3 StartPos;
        public Vector3 StartScale;
        public Quaternion StartRot;
    }
    ClickInfo clickee;
    Camera    cam;
    void Update () {
        if (cam == null)
            cam = Camera.main;

        // distance from ceiling to floor 
        var lDist = Vector3.Distance(gos[0].transform.position, gos[1].transform.position);
        var rDist = Vector3.Distance(gos[3].transform.position, gos[4].transform.position);
        var lMid =  Vector3.Lerp(gos[0].transform.position, gos[1].transform.position, 0.5f);
        var rMid =  Vector3.Lerp(gos[3].transform.position, gos[4].transform.position, 0.5f);
        var midOfLAndRMids = Vector3.Lerp(lMid, rMid, 0.5f);

        var frac = Mathf.Min(lDist, rDist) / 
                   Mathf.Max(lDist, rDist);

        gos[mid].transform.position = new Vector3(
            midOfLAndRMids.x,//lWallEdge + wallWid * frac,
            midOfLAndRMids.y,
            midOfLAndRMids.z);


        if (!draggedOrResized()) {
            ;
        }



        bool draggedOrResized () {
            // WARNING FOR ANY REFACTORING... can't do Mouse0 & Mouse2 functionality simultaneously, as they use the same clickee spot for state 
            if (Input.GetKeyUp(KeyCode.Mouse0) ||
                Input.GetKeyUp(KeyCode.Mouse2)) 
            {
                clickee = null;
                planeGO.gameObject.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) ||
                Input.GetKeyDown(KeyCode.Mouse2)) 
            {
                if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {
                    if (hit.transform.name != "Cube") {
                        //planeGO.gameObject.SetActive(true);
                        planeGO.transform.position = hit.point;
                        planeGO.transform.up = hit.normal;
                    }

                    clickee = new ClickInfo {
                        Plane      = new Plane(hit.normal, hit.point),
                        Hit        = hit,
                        StartPos   = hit.transform.position, // the latter will get altered per-frame, anchored on StartPos 
                        StartScale = hit.transform.localScale,
                        StartRot   = hit.transform.rotation,
                    };

                    //Debug.Log("hit");
                    return true;
                }
            }

            if (clickee != null) { // (editing key held after valid surface click) 
                if (Input.GetKey(KeyCode.Mouse0)) { // LMB 
                    var ray = cam.ScreenPointToRay(Input.mousePosition);

                    if (clickee.Plane.Raycast(ray, out float hitDist)) {
                        var v = ray.origin + ray.direction * hitDist;
                        moveStraightInY(v);
                        //gos[mid].transform.position = v;

                        var light  = (gos[3].transform.position - gos[0].transform.position)           ; // left blue, right white  
                        var onNorm = (gos[4].transform.position - gos[1].transform.position).normalized; // left white, right blue
                        gos[mid].transform.position = gos[0].transform.position + Vector3.Project(light, onNorm);
                    }
                }

                if (Input.GetKey(KeyCode.Mouse2)) { // MMB 
                    var ray = cam.ScreenPointToRay(Input.mousePosition);

                    if (clickee.Plane.Raycast(ray, out float hitDist)) {
                        var v = ray.origin + ray.direction * hitDist;
                        resize(v);
                        gos[mid].transform.position = v;
                    }
                }
            }



            void moveStraightInY (Vector3 newSpot) {
                var del = newSpot - clickee.Hit.point;
                //del.x = 0f;
                //del.z = 0f;
                clickee.Hit.transform.position = // touched object updated per frame... 
                clickee.StartPos + del; //...anchored to cached point where click started 
            }


            // ONLY WORKS PROPERLY ON IDENTITY ORIENTED QUADS (& when cam aiming mostly north or south) 
            void resize (Vector3 newSpot) {
                var del = newSpot - clickee.Hit.point;
                //del.z = 0f;

                float x  = 0f;
                float y  = 0f;
                float xS = 0f;
                float yS = 0f;

                if (clickee.Hit.point.x < clickee.StartPos.x) {
                    x  = -(-del.x * 0.5f);
                    xS =   -del.x;
                } else {
                    x  = del.x * 0.5f;
                    xS = del.x;
                }

                if (clickee.Hit.point.y < clickee.StartPos.y) {
                    y  = -(-del.y * 0.5f);
                    yS =   -del.y;
                } else {
                    y  = del.y * 0.5f;
                    yS = del.y;
                }

                clickee.Hit.transform.position   = clickee.StartPos   + new Vector3(x,   y, 0f);
                clickee.Hit.transform.localScale = clickee.StartScale + new Vector3(xS, yS, 0f); ;
                clickee.Hit.transform.localScale = new Vector3(
                    Mathf.Abs(clickee.Hit.transform.localScale.x), 
                    Mathf.Abs(clickee.Hit.transform.localScale.y),
                    Mathf.Abs(clickee.Hit.transform.localScale.z));
            }

            return false;
        }
    }
}
