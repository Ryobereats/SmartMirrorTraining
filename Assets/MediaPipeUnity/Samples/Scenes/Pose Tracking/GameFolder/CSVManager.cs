using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVManager : MonoBehaviour
{
  public static CSVManager Instance;

  private void Awake()
  {
    Instance = this;
  }

  public string GetFolderPath(string userName)
  {
    DateTime now = DateTime.Now;
    string nowTime = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString() + "__" + now.Hour.ToString() + "_" + now.Minute.ToString() + "_" + now.Second.ToString();
    string folderPath = Application.persistentDataPath + "/" + userName + "_" + nowTime;
    return folderPath;
  }

  public void CreateDirectory(string path)
  {
    Directory.CreateDirectory(path);
  }

  /// <summary>
  /// ���l�߂�csv�ɋL��
  /// ",":�s���E�Ɉړ�  "\n"������Ɉړ�
  /// </summary>
  /// <param name="data"></param>
  /// <param name="fileName"></param>
  //public void SaveData(string path, string data, string fileName)
  //{
  //    StreamWriter sw;
  //    FileInfo fi;

  //    fi = new FileInfo(path + "/" + fileName + ".csv");
  //    FileStream fs = fi.Create();
  //    sw = fi.AppendText();
  //    sw.WriteLine(data);
  //    sw.Flush();
  //    sw.Close();
  //}
  public void SaveData(string path, string data, string fileName)
  {
    StreamWriter sw;
    FileInfo fi;

    fi = new FileInfo(path + "/" + fileName + ".csv");

    sw = fi.AppendText();
    sw.WriteLine(data);
    sw.Flush();
    sw.Close();
    //Debug.Log(path + "/" + fileName + ".csv" + data + "を保存");
  }
}
