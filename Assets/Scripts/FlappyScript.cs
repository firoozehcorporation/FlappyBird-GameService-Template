using System;
using UnityEngine;
using FiroozehGameServiceAndroid;
using FiroozehGameServiceAndroid.Builders;
using Newtonsoft.Json;
using UnityEngine.UI;

/// <summary>
/// Spritesheet for Flappy Bird found here: http://www.spriters-resource.com/mobile_phone/flappybird/sheet/59537/
/// Audio for Flappy Bird found here: https://www.sounds-resource.com/mobile/flappybird/sound/5309/
/// </summary>
public class FlappyScript : MonoBehaviour
{

    public AudioClip FlyAudioClip, DeathAudioClip, ScoredAudioClip;
    public Sprite GetReadySprite;
    public float RotateUpSpeed = .5f, RotateDownSpeed = .5f;
    public GameObject IntroGUI, DeathGUI;
    public Collider2D restartButtonGameCollider;
    public float VelocityPerJump = 3;
    public float XSpeed = 1;
    
    

    // Game Service property
    public static FiroozehGameService _gameService;
    private string Error;
    private string Res;

    public Button play;
    public Button LeaderBord;
    public Button Achievement;
    public Button Survey;
    
    
    // Use this for initialization
    void Start()
    {
        
        play.onClick.AddListener(()=>
        {
            BoostOnYAxis();
            GameStateManager.GameState = GameState.Playing;
            IntroGUI.SetActive(false);
        });
        
        LeaderBord.onClick.AddListener(()=>
        {
            _gameService?.ShowLeaderBoardsUI(e =>
            {
                Error = "ShowLeaderBoardsUI Error : "+e;
            });
        });
        
        Achievement.onClick.AddListener(()=>
        {
            _gameService?.ShowAchievementsUI(e =>
            {
                Error = "ShowAchievementsUI Error : "+e;
            });
        });
        
        Survey.onClick.AddListener(()=>
        {
            _gameService?.ShowSurveyUi(e =>
            {
                Error = "ShowSurveyUi Error : "+e;
            });
        });
        
        
        if(_gameService == null)
        FiroozehGameServiceInitializer
            .With("Your clientId","Your clientSecret")
            .IsNotificationEnable(true)
            .CheckGameServiceOptionalUpdate(true)
            .CheckGameServiceInstallStatus(true)
            .Init(g =>
                {
                    _gameService = g;
                                        
                    _gameService?.DownloadObbData("main.VersionCode.<PackageName>.obb", r =>
                        {
                            if (r.Equals("Data_Download_Finished") || r.Equals("Data_Downloaded"))
                            {
                                //Now Data Exist!! , Load Base Scenes
                            }     
                      
                        },
                        e =>
                        {
                            if(e.Equals("Data_Download_Dismissed"))
                                Application.Quit();
                            
                            Error = "DownloadObbData Error : " + e;
                        });
                    
                   

                    
                    
                    _gameService?.GetSaveGame(c =>
                    {
                        Save save = JsonConvert.DeserializeObject<Save>(c);
                        ScoreManagerScript.Score = save.Score;

                    }
                        , e => {
                        Error = "GetSaveGame Error : " + e;

                    });
                    
                   
                    
                }, 
                e =>
                {
                    Debug.Log("FiroozehGameServiceInitializerError: "+e);
                });
        
      

    }

    FlappyYAxisTravelState flappyYAxisTravelState;

    enum FlappyYAxisTravelState
    {
        GoingUp, GoingDown
    }

    Vector3 birdRotation = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        //handle back key in Windows Phone
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        switch (GameStateManager.GameState)
        {
            case GameState.Intro:
                break;
            case GameState.Playing:
            {
                MoveBirdOnXAxis();
                if (WasTouchedOrClicked())
                {
                    BoostOnYAxis();
                }

                break;
            }
            case GameState.Dead:
            {
                Vector2 contactPoint = Vector2.zero;

                if (Input.touchCount > 0)
                    contactPoint = Input.touches[0].position;
                if (Input.GetMouseButtonDown(0))
                    contactPoint = Input.mousePosition;

                //check if user wants to restart the game
                if (Camera.main != null && restartButtonGameCollider == Physics2D.OverlapPoint
                        (Camera.main.ScreenToWorldPoint(contactPoint)))
                {
                    GameStateManager.GameState = GameState.Intro;
                    Application.LoadLevel(Application.loadedLevelName);
                }

                break;
            }
        }
    }


    void FixedUpdate()
    {
        switch (GameStateManager.GameState)
        {
            //just jump up and down on intro screen
            case GameState.Intro:
            {
                if (GetComponent<Rigidbody2D>().velocity.y < -1) //when the speed drops, give a boost
                    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 5500 * Time.deltaTime)); //lots of play and stop 
                //and play and stop etc to find this value, feel free to modify
                break;
            }
            case GameState.Playing:
            case GameState.Dead:
                FixFlappyRotation();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    bool WasTouchedOrClicked()
    {
        return Input.GetButtonUp("Jump") || Input.GetMouseButtonDown(0) || 
               Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended;
    }

    void MoveBirdOnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    void BoostOnYAxis()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, VelocityPerJump);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }



    /// <summary>
    /// when the flappy goes up, it'll rotate up to 45 degrees. when it falls, rotation will be -90 degrees min
    /// </summary>
    private void FixFlappyRotation()
    {
        flappyYAxisTravelState = GetComponent<Rigidbody2D>().velocity.y > 0 ? FlappyYAxisTravelState.GoingUp : FlappyYAxisTravelState.GoingDown;

        float degreesToAdd = 0;

        switch (flappyYAxisTravelState)
        {
            case FlappyYAxisTravelState.GoingUp:
                degreesToAdd = 6 * RotateUpSpeed;
                break;
            case FlappyYAxisTravelState.GoingDown:
                degreesToAdd = -3 * RotateDownSpeed;
                break;
        }
        //solution with negative eulerAngles found here: http://answers.unity3d.com/questions/445191/negative-eular-angles.html

        //clamp the values so that -90<rotation<45 *always*
        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }

    /// <summary>
    /// check for collision with pipes
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState != GameState.Playing) return;
        if (col.gameObject.CompareTag("Pipeblank")) //pipeblank is an empty gameobject with a collider between the two pipes
        {
            GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
            ScoreManagerScript.Score++;

            if (ScoreManagerScript.Score % 5 == 0)
                XSpeed += XSpeed*.1f;

            switch (ScoreManagerScript.Score)
            {
               case  5:
                {
                    _gameService?.UnlockAchievement("FIVE_SCORE", c =>
                    {
                        Res = "Five Score Achievement Unlocked!!";
                    }, e =>
                    {
                        Error = "UnlockAchievement Error : "+e;
                    });
                    break;    
                }
                case 10:
                {
                    _gameService?.UnlockAchievement("TEN_SCORE", c =>
                    {
                        Res = "Ten Score Achievement Unlocked!!";
                    }, e =>
                    {
                        Error = "UnlockAchievement Error : "+e;
                    });
                    break;
                }
                case 15:
                {
                    _gameService?.UnlockAchievement("FIFTEEN_SCORE", c =>
                    {
                        Res = "Fifteen Score Achievement Unlocked!!";
                    }, e =>
                    {
                        Error = "UnlockAchievement Error : "+e;
                    });
                    
                    break;
                }
                case 20:
                {
                    _gameService?.UnlockAchievement("TWENTY_SCORE", c =>
                    {
                        Res = "Twenty Score Achievement Unlocked!!";
                    }, e =>
                    {
                        Error = "UnlockAchievement Error : "+e;
                    });

                    
                    break;
                }

                default:
                {
                    if(ScoreManagerScript.Score > 20 && ScoreManagerScript.Score % 5 == 0)
                    _gameService?.SubmitScore("FloppyBird_List",ScoreManagerScript.Score
                        , c =>
                        {
                            Res = "SubmitScore with "+ScoreManagerScript.Score+" Saved!!";

                        }, e =>
                        {
                            Error = "SubmitScore Error : " + e;
                            
                        });
                    break;
                }
                   
            }
            
            
            
            
        }
        else if (col.gameObject.CompareTag("Pipe"))
        {
            FlappyDies();
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState != GameState.Playing) return;
        if (!col.gameObject.CompareTag("Floor")) return;
        
        FlappyDies();
    }

    void FlappyDies()
    {
        Save save = new Save();
        save.Score = ScoreManagerScript.Score;

        _gameService?.SaveGame("FloppyBird_SAVE_"+Time.time,
            "FloppyBirdSaveDone"
            ,null,JsonConvert.SerializeObject(save),
            c =>
            {
                    
                Res = "SaveGame with "+ScoreManagerScript.Score+" Done!!";

            }, e =>
            {
                Error = "SaveGame Error : " + e;
            });

        
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);
        
        
    }
    
    /// <summary>
    /// Found here
    /// http://www.bensilvis.com/?p=500
    /// </summary>
    /// <param name="screenWidth"></param>
    /// <param name="screenHeight"></param>
    public static void AutoResize(int screenWidth, int screenHeight)
    {
        Vector2 resizeRatio = new Vector2((float)Screen.width / screenWidth, (float)Screen.height / screenHeight);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));
    }

    void OnGUI()
    {
        GUI.Label(new Rect(700, 50, 200, 100), "Response: " + Res);
        GUI.Label(new Rect(50, 400, 500, 500), "Error " + Error);

    }

}
