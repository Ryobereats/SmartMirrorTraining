using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamManager : MonoBehaviour
{
  //2023/9/18(月)追加
  private WebCamTexture _webCamTexture;
  private Renderer _renderer;

  // Start is called before the first frame update
  //void Start()
  //{
  //  //WebCamDevice[] devices = WebCamTexture.devices;
  //  //string displayCamName = "Laptop_Integrated_Webcam";

  //  //Debug.Log(devices[0] + "マリオ1");
  //  //Debug.Log(devices[1] + "マリオ2");
  //  //if (devices != null)
  //  //{
  //  //  _webCamTexture = new WebCamTexture(displayCamName, Screen.width, Screen.height);
  //  //  _renderer.material.mainTexture = _webCamTexture;
  //  //  _webCamTexture.Play();
  //  //}

  //  //else
  //  //{
  //  //  Debug.Log("Webカメラが見つかりませんでした。");
  //  //}

  //}

  int width = 1920;
  int height = 1080;
  int fps = 60;
  WebCamTexture webcamTexture;
  string displayCamName = "Laptop_Integrated_Webcam";
  void Start()
  {
    WebCamDevice[] devices = WebCamTexture.devices;
    webcamTexture = new WebCamTexture(displayCamName, this.width, this.height, this.fps);
    GetComponent<Renderer>().material.mainTexture = webcamTexture;
    webcamTexture.Play();
    this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.transform.localScale.y * -1, this.transform.localScale.z);

    Debug.Log(devices[0].name + "カメラ1");
    Debug.Log(devices[1].name + "カメラ2");
  }
    // Update is called once per frame
    void Update()
  {

  }
}
