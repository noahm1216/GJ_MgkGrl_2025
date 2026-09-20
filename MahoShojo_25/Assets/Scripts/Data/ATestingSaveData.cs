using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ATestingSaveData : MonoBehaviour
{
    [SerializeField] // Name, Type, Data
    public List<SavaDataContent> dataCollection { get; private set; } = new List<SavaDataContent>();

    public void Start()
    {
        print("Press g + h to save || press g + j to load and apply");
        SavaDataContent _testSaveContent = new SavaDataContent($"{transform.name}_position", typeof(Vector3), transform.position.ToString());
        WriteData<Vector3>(_testSaveContent);
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.G))
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                SavaDataContent _testSaveContent = new SavaDataContent($"{transform.name}_position", typeof(Vector3), transform.position.ToString());
                WriteData<Vector3>(_testSaveContent);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                SavaDataContent _testLoadContent =  ReadData($"{transform.name}_position");
                if (_testLoadContent != null) InterpretData(_testLoadContent);
            }
        }
    }

    public void WriteData<T>(SavaDataContent _data) // write entry to save (overrites if existing overlaps)
    {
        print($"able to pass data: {_data.nickname} - {_data.type} - {_data.value}");

        for (int i = 0; i < dataCollection.Count; i++) // check our data for a reference to update
        {
            if (_data.nickname == dataCollection[i].nickname)
            {
                dataCollection[i] = _data;
                _data = null; // empty the reference so we dont have to create extra memory to track
                print("Data updated as existing entry");
                break;
            }
        }
        if (_data != null) { dataCollection.Add(_data); print("Data Added as new entry"); } // adds the data if we didnt clear it/find the match

    }

    public void CollectSaveData()
    {
        // look for a save file (or pass one)
        // run through and interpret the data into our data collection for runtime reference // (NOTE: this could be bad for low end machines or data garbage) maybe we decide to read a file at runtime which is more computation heavy
        // once we collect it, everything else can ask for it's state
    }

    public SavaDataContent ReadData(string _keyName)
    {
        for (int i = 0; i < dataCollection.Count; i++) // check our data for a reference to update
            if (_keyName == dataCollection[i].nickname)
                return dataCollection[i];

        Debug.LogWarning($"NO DATA FOUND FOR NAME: {_keyName}");
        return null;
    }

    //DELETE THIS AFTER TESTING
    public void InterpretData(SavaDataContent _data)
    {
        if (_data == null) return;

        if (_data.type == typeof(Vector3)) // would need to check for every time we want to save (and have a converter for string)
        {
            Vector3 _convert = StringToVector3(_data.value);
            ApplyTransform(transform, _convert);
        }
    }

    public void ApplyTransform(Transform _obj, Vector3 _pos)
    {
        if (_obj) _obj.position = _pos;
    }
    // END OF DELETE THIS

    public static Vector3 StringToVector3(string rString)
    {
        // Remove parentheses if they exist
        if (rString.StartsWith("(") && rString.EndsWith(")"))
        {
            rString = rString.Substring(1, rString.Length - 2);
        }

        // Split the items by comma
        string[] sArray = rString.Split(',');

        // Ensure we have exactly 3 components
        if (sArray.Length != 3)
        {
            Debug.LogError($"Invalid Vector3 string format: {rString}");
            return Vector3.zero;
        }

        // Parse with InvariantCulture to safely process decimal points (.) regardless of user region
        float x = float.Parse(sArray[0], CultureInfo.InvariantCulture);
        float y = float.Parse(sArray[1], CultureInfo.InvariantCulture);
        float z = float.Parse(sArray[2], CultureInfo.InvariantCulture);

        return new Vector3(x, y, z);
    }
}


[System.Serializable]
public class SavaDataContent
{
    public string nickname;
    public Type type;
    public string value; 

    public SavaDataContent (string _nickname, Type _type, string _value)
    {
        nickname = _nickname;
        type = _type;
        value = _value;
    }

}
