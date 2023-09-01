using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class CoinManager : MonoBehaviour
{
  public static CoinManager Instance;
  private GameManager _gameManager;
  //private ScoreManager _scoreManager;

  [SerializeField]
  private ParticleSystem normalParticle;

  [SerializeField]
  private ParticleSystem specialParticle;

  [SerializeField]
  private AudioClip normalSe;

  [SerializeField]
  private AudioClip specialSe;


  [SerializeField]
  private GameObject glassesImage;

  public float consecutiveBonus { get; private set; } = 0.43f;
  private float coinRadius = 0.025f;

  //1.25追加
  public float accuracyAmp { get; private set; } = 1.1f;
  //public float consecutiveRate = 0;
  //public float accuracy = 0;

  private void Awake()
  {
    Instance = this;
  }
  // Start is called before the first frame update
  void Start()
  {
    //_saveParameter = SaveParameter.reference_SP;
    _gameManager = GameManager.Instance;
    //_scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
  }

  // Update is called once per frame
  void LateUpdate()
  {
    transform.Rotate(new Vector3(0, 0, 5));

    //if (_saveParameter.TrainingTypes != 4)
    //{
    //    if (this.gameObject.transform.position.z <= -5) Destroy();
    //}
    //else
    //{
    //    if (this.gameObject.transform.position.x >= 5) Destroy();
    //}
  }

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("衝突" + other.tag);
    //HeadColliderと衝突したらコイン削除
    if (other.name == "HeadCollider")
    {
      float y = transform.position.y;
      float Y = other.transform.position.y;
      float distanceY = Mathf.Abs(y - Y);

      float z = transform.position.z;
      float Z = other.transform.position.z;
      float distanceZ = Mathf.Abs(z - Z);

      StartParticle(normalParticle);
      StartSound(normalSe);
      Debug.Log("コイン取得");
      float addPoint;

      if (this.gameObject.tag == "GoldCoin")
      {
        addPoint = 1;
        //_scoreManager.AddCoin(ScoreManager.CoinType.GoldCoin);
        Debug.Log("AddCoin(ScoreManager.CoinType.GoldCoin)が実行されました");
      }
      else
      {
        addPoint = 2;
        //_scoreManager.AddCoin(ScoreManager.CoinType.RedCoin);
        Debug.Log("AddCoin(ScoreManager.CoinType.RedCoin)が実行されました");
      }

      if (_gameManager.trainingTypes != 4)
      {
        //addPoint = CheckDistance(addPoint, distanceY);
        //_scoreManager.Score += addPoint;
        this.gameObject.SetActive(false);
        Debug.Log("コインが消えました！！！");
      }
      else
      {
        //addPoint = CheckDistance(addPoint, distanceZ);
        //_scoreManager.Score += addPoint;
        this.gameObject.SetActive(false);
      }
    }
  }

  //private void Func()//安定性ボーナス
  //{
  //    List<GameObject> coinList = _gameManager.Coins;
  //    int index = coinList.IndexOf(this.gameObject);
  //    int oneCycle = _gameManager.oneCycle;
  //    int count = 0;
  //    if (index !=0 && (index+1) % oneCycle == 0)
  //    {
  //        for(int i =index;i>(index + 1) - oneCycle; --i)
  //        {
  //            if (coinList[i] == null) ++count;
  //        }

  //        if(count == oneCycle)//次の周期でコインゲットした時にボーナスがもらえる
  //    }
  //}

  //private float CheckDistance(float addPoint, float distance) //連続正解性ボーナス
  //{

  //  ////追加したいもの
  //  //Debug.Log("indexの値は" + index + ("です"));
  //  int num = this.gameObject.transform.GetSiblingIndex();
  //  Debug.Log("num=" + num + "の時の" + "isConsecutiveの値は、、、" + _scoreManager.isConsecutive + "です。。。。");

  //  if (num != 0)
  //  {
  //    Debug.Log("前のインデックスのアクティブは、、、" + this.gameObject.transform.parent.gameObject.transform.GetChild(num - 1).gameObject.activeSelf);
  //  }

  //  if (distance <= coinRadius / 2)
  //  {
  //    Debug.Log("_scoreManager.GoodCountは、" + _scoreManager.GoodCount + "です！！！！");
  //    _scoreManager.GoodCount++;
  //    addPoint *= accuracyAmp;
  //    StartParticle(specialParticle);
  //    StartSound(specialSe);

  //    if (num == 0)
  //    {
  //      _scoreManager.isConsecutive = true;
  //      return addPoint;
  //    }
  //    if (transform.parent.GetChild(num - 1).gameObject.activeSelf == false && _scoreManager.isConsecutive)
  //    {
  //      Debug.Log("_scoreManager.ConsecutiveCount" + _scoreManager.ConsecutiveCount + "です！！！");
  //      addPoint += consecutiveBonus;
  //      //GameObject effectImage = Instantiate(glassesImage, this.transform.position, Quaternion.identity).gameObject;
  //      //Destroy(effectImage, 2);
  //      _scoreManager.AddCoin(ScoreManager.CoinType.GlassesImages);
  //    }
  //    _scoreManager.isConsecutive = true;
  //  }
  //  else
  //  {
  //    StartParticle(normalParticle);
  //    StartSound(normalSe);
  //    _scoreManager.isConsecutive = false;
  //  }

  //  return addPoint;
  //}

  private void Destroy()
  {
    Destroy(gameObject);
  }

  private void StartParticle(ParticleSystem particle)
  {
    //パーティクル用ゲームオブジェクト生成
    GameObject effect_particle = Instantiate(particle, new Vector3(this.transform.position.x,this.transform.position.y,60), Quaternion.identity).gameObject;
    effect_particle.transform.localScale = this.transform.localScale /100 ;

    if(effect_particle.transform.localScale.y < 0)
    {
      effect_particle.transform.localScale = new Vector3(effect_particle.transform.localScale.x,
                                                         effect_particle.transform.localScale.y * (-1),
                                                         effect_particle.transform.localScale.z);
    }
    //Debug.Log(effect_particle.transform.localScale);
    Destroy(effect_particle, 2);

  }


  private void StartSound(AudioClip se)
  {
    AudioManager.Instance.PlaySE(AudioManager.SE.NormalSe);
  }

}
