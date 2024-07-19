using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Text_Manager
{
    //객체의 이름을 stringㅇ로 받으면 text 정보를 string으로 반환
    public string get_Text(string object_string)
    {
        string file_path = Application.dataPath + "/Resources/Text_Folder";
        file_path = file_path + "/" + object_string + ".txt";
        Debug.Log(file_path);
        string value = "";
        
        FileInfo file = new FileInfo(file_path);
        if(file.Exists)
        {
            StreamReader reader = new StreamReader(file_path);
            value = reader.ReadToEnd();
            reader.Close();
        }
        else
        {
            value = "No exist file";
        }

        return value;
    }
}