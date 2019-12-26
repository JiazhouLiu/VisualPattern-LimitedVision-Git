using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterCubeScript : MonoBehaviour
{
    public float radius;
    public float degreeOfView;
    public Transform manager;

    public bool CollisionDetection = false;

    private Layout oldLayout = Layout.NULL;

    private void Start()
    {
        transform.localScale = new Vector3(degreeOfView * Mathf.PI * radius / 180, StartSceneScript.adjustedHeight + 1.3f, 2 * radius);

        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        int[] triangles = GetComponent<MeshFilter>().mesh.triangles;
        for (int i = 6; i < 12; i++) {
            triangles[i] = 0;
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.transform.position;
        transform.position = new Vector3(transform.position.x, (StartSceneScript.adjustedHeight + 1.25f) / 2 - 0.05f, transform.position.z);

        if (manager.GetComponent<ExperimentManager>().layout != oldLayout) {
            oldLayout = manager.GetComponent<ExperimentManager>().layout;
        }

        if (oldLayout == Layout.LimitedFlat)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
        }
    }
}
