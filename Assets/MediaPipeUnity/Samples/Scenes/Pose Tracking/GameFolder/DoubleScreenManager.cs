using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleScreenManager : MonoBehaviour//2023/9/28(木)追加
{

  [SerializeField] private GameObject MainCamera;
  [SerializeField] private GameObject AllUI;
  [SerializeField] private GameObject MirrorCalibText;//2023/10/4水追加
  [SerializeField] private GameObject Footer;
  [SerializeField] private GameObject WebCamManager;

  //2023/10/11(水)追加
  //画面表示縦横関連UI調整
  [SerializeField] private GameObject CameraSideButton;
  [SerializeField] private GameObject CameraBackButton;
  [SerializeField] private GameObject CameraUpButton;
  [SerializeField] private GameObject CameraTypeText;
  [SerializeField] private GameObject CameraAllViewButton;

  private Vector3 _verticalScreenCameraPos;
  private Vector3 _horizontalScreenCameraPos = new Vector3(103.3f,1,-10);
  private Vector3 _verticalAllUIPos;
  private Vector3 _initMirrorCalibTextSize;//2023/10/4水追加
  private Vector3 _verticalFooterPos;
  private Vector3 _verticalFooterScale;

  //WebCamを取り扱うための変数
  private int width = 1920;
  private int height = 1080;
  private int fps = 60;
  private WebCamTexture webcamTexture;
  private string displayCamName = "Laptop_Integrated_Webcam";
  private WebCamDevice[] _webCamDevices;


  //Footerについて

  void Start()
  {
    _verticalScreenCameraPos = MainCamera.transform.position;//スマートミラー縦表示の時のメインカメラの位置を保存
    _verticalAllUIPos = AllUI.transform.position;//ALLUIの縦表示の時の位置を保存
    _initMirrorCalibTextSize = MirrorCalibText.transform.localScale;//2023/10/4水追加
    _verticalFooterPos = Footer.transform.position;//Footerの縦表示の時の位置を保存
    _verticalFooterScale = Footer.transform.localScale;//2023/10/4水追加

    //WebCamを取り扱う際の処理

    _webCamDevices = WebCamTexture.devices;
    webcamTexture = new WebCamTexture(displayCamName, this.width, this.height, this.fps);
    WebCamManager.GetComponent<Renderer>().material.mainTexture = webcamTexture;
    webcamTexture.Play();
    WebCamManager.transform.localScale = new Vector3(WebCamManager.transform.localScale.x * -1, WebCamManager.transform.transform.localScale.y, WebCamManager.transform.localScale.z);

    for(int i = 0; i < _webCamDevices.Length; i++)
    {
      Debug.Log(_webCamDevices[i].name + "カメラ" + i);
    }
  }
  void Update()
  {

  }

 
  public void SwitchSmartMirrorTypeToHorizontal()//スマートミラーの縦横表示を切り替える　横
  {
    List<Transform> childTransformList = new List<Transform>();//Footer配下の4つのボタンのtransformをリストに格納
    
    //MainCameraの位置変更
    MainCamera.transform.position = new Vector3(103.3f, 1, -10);

    //MainCanvas配下のAllUIとFooterを右に寄せる
    AllUI.transform.position = new Vector3(49, 1, 90);

    //MirrorCalibTextの大きさを小さくする 2023/10/4水追加
    MirrorCalibText.transform.localScale = new Vector3(_initMirrorCalibTextSize.x * 0.5f, _initMirrorCalibTextSize.y, _initMirrorCalibTextSize.z);

    //Footerについて
    Footer.transform.localPosition = new Vector3(586, -664, -59);//Footer配下の4つのボタンの位置が右中心に寄るように移動、Footerの横幅が大きく右にはみ出てしまう
    for(int i = 0;i < Footer.transform.childCount;i++)
    {
      Debug.Log("チャイルド" + Footer.transform.GetChild(i).name);
      childTransformList.Add(Footer.transform.GetChild(i));
    }
    Footer.transform.localScale = new Vector3(0.52f, 1, 1);//Footerの横幅を半分にする
    for (int i = 0; i < Footer.transform.childCount; i++)//Footer配下の4つのボタンの大きさは変えない
    {
      Footer.transform.GetChild(i).localScale = childTransformList[i].localScale;
    }
    CameraSideButton.SetActive(true);
    CameraBackButton.SetActive(true);
    CameraUpButton.SetActive(true);
    CameraTypeText.SetActive(true);
    CameraAllViewButton.SetActive(true);
  }
  public void SwitchSmartMirrorTypeToVertical()//スマートミラーの縦横表示を切り替える 縦
  {
    //MainCameraの位置変更
    MainCamera.transform.position = _verticalScreenCameraPos;

    //MainCanvas配下のAllUIとFooterを元の位置に戻す
    AllUI.transform.position = _verticalAllUIPos;
    Footer.transform.position = _verticalFooterPos;
    Footer.transform.localScale = _verticalFooterScale;

    //MirrorCalibTextの大きさを元に戻す 2023/10/4水追加
    MirrorCalibText.transform.localScale = _initMirrorCalibTextSize;
    //GameWindowのAspect変更（40:21,Aspect）

    CameraSideButton.SetActive(false);
    CameraBackButton.SetActive(false);
    CameraUpButton.SetActive(false);
    CameraTypeText.SetActive(false);
    CameraAllViewButton.SetActive(false);
  }

  public void SwitchDisplayViewToSide()//ディスプレイのカメラを横映像に
  {
    if(displayCamName != _webCamDevices[1].name)
    {
      webcamTexture.Stop();
      displayCamName = _webCamDevices[1].name;
      webcamTexture = new WebCamTexture(_webCamDevices[1].name, this.width, this.height, this.fps);//配列のインデックスは変わる予定
      Debug.Log("カメラデバッグ横webcamTextureがセットされたよ");

      WebCamManager.GetComponent<Renderer>().material.mainTexture = webcamTexture;
      Debug.Log("カメラデバッグ横WebCamManagerのテクスチャをセットしたよ");

      webcamTexture.Play();
      Debug.Log("カメラデバッグ横webcamTextureが再生されたよ");

      WebCamManager.transform.localScale = new Vector3(WebCamManager.transform.localScale.x * -1, WebCamManager.transform.transform.localScale.y, WebCamManager.transform.localScale.z);
      Debug.Log("カメラデバッグ横WebCamManagerの大きさが変化したよ");
    }
  }

  public void SwitchDisplayViewToBack()//ディスプレイのカメラを後ろ映像に
  {
    if (displayCamName != _webCamDevices[2].name)
    {
      webcamTexture.Stop();
      displayCamName = _webCamDevices[2].name;
      webcamTexture = new WebCamTexture(_webCamDevices[2].name, this.width, this.height, this.fps);
      Debug.Log("カメラデバッグ後webcamTextureがセットされたよ");

      WebCamManager.GetComponent<Renderer>().material.mainTexture = webcamTexture;
      Debug.Log("カメラデバッグ後WebCamManagerのテクスチャをセットしたよ");

      webcamTexture.Play();
      Debug.Log("カメラデバッグ後webcamTextureが再生されたよ");

      WebCamManager.transform.localScale = new Vector3(WebCamManager.transform.localScale.x * -1, WebCamManager.transform.transform.localScale.y, WebCamManager.transform.localScale.z);
      Debug.Log("カメラデバッグ後WebCamManagerの大きさが変化したよ");
    }
  }

  public void SwitchDisplayViewToUp()//ディスプレイのカメラを上映像に
  {
    if (displayCamName != _webCamDevices[3].name)
    {
      webcamTexture.Stop();
      displayCamName = _webCamDevices[3].name;
      webcamTexture = new WebCamTexture(_webCamDevices[3].name, this.width, this.height, this.fps);
      Debug.Log("カメラデバッグ上webcamTextureがセットされたよ");

      WebCamManager.GetComponent<Renderer>().material.mainTexture = webcamTexture;
      Debug.Log("カメラデバッグ上WebCamManagerのテクスチャをセットしたよ");

      webcamTexture.Play();
      Debug.Log("カメラデバッグ上webcamTextureが再生されたよ");

      WebCamManager.transform.localScale = new Vector3(WebCamManager.transform.localScale.x * -1, WebCamManager.transform.transform.localScale.y, WebCamManager.transform.localScale.z);
      Debug.Log("カメラデバッグ上WebCamManagerの大きさが変化したよ");
    }
  }

  public void SwitchDisplayViewToAll()//ディスプレイのカメラを3視点映像に
  {
    if (displayCamName != _webCamDevices[1].name)
    {
      webcamTexture.Stop();
      displayCamName = _webCamDevices[1].name;
      webcamTexture = new WebCamTexture(_webCamDevices[1].name, this.width, this.height, this.fps);//配列のインデックスは変わる予定
      Debug.Log("カメラデバッグ横webcamTextureがセットされたよ");

      WebCamManager.GetComponent<Renderer>().material.mainTexture = webcamTexture;
      Debug.Log("カメラデバッグ横WebCamManagerのテクスチャをセットしたよ");

      webcamTexture.Play();
      Debug.Log("カメラデバッグ横webcamTextureが再生されたよ");

      WebCamManager.transform.localScale = new Vector3(WebCamManager.transform.localScale.x * -1, WebCamManager.transform.transform.localScale.y, WebCamManager.transform.localScale.z);
      Debug.Log("カメラデバッグ横WebCamManagerの大きさが変化したよ");
    }
  }

}
