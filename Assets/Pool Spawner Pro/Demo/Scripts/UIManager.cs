using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool isAreaSpawn;
    [SerializeField] private Text position;
    [SerializeField] private Text area;
    [SerializeField] private AreaSpawn areaSpawn;

    private void Update()
    {
        if (!isAreaSpawn)
            position.text = transform.position.z.ToString("F1");
        else
            area.text = areaSpawn.areaProperties.area.x.ToString() + " x " + areaSpawn.areaProperties.area.y.ToString();

    }
}
