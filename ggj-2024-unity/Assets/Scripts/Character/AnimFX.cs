using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimFXObject
{
    public string FXKey = "MyFXObj";
    public GameObject FXObj;
    public Transform SpawnLocation = null;
    public bool ParentFX = false;
}

public class AnimFX : MonoBehaviour
{
    public List<AnimFXObject> FXObjects = new List<AnimFXObject>();

    public void PlayAnimFX(string FXKeyToPlay)
    {
        AnimFXObject choice = GetFXObj(FXKeyToPlay);

        if(choice != null && choice.FXObj != null)
        {
            if(choice.ParentFX && choice.SpawnLocation != null)
            {
                Instantiate(choice.FXObj, choice.SpawnLocation.position, choice.FXObj.transform.rotation, choice.SpawnLocation);
            }
            else if(choice.SpawnLocation != null)
            {
                Instantiate(choice.FXObj, choice.SpawnLocation.position, choice.FXObj.transform.rotation);
            }
            else
            {
                Instantiate(choice.FXObj, transform.position, choice.FXObj.transform.rotation);
            }
        }
    }

    public AnimFXObject GetFXObj(string FXObjKey)
    {
        List<AnimFXObject> results = new List<AnimFXObject>();
        foreach (AnimFXObject fxo in FXObjects)
        {
            if(fxo.FXKey == FXObjKey) results.Add(fxo);
        }

        if(results.Count>0)
        {
            return results[Random.Range(0,results.Count)];
        }
        else return null;
    }
}
