using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesertRacerGame : MonoBehaviour
{
    public static DesertRacerGame Instance;
    public float roadLength = 900f;
    public float roadWidth = 18f;
    public Transform player;
    public float speed = 0f;
    public float maxSpeed = 52f;
    public int laps = 0;
    public int totalLaps = 3;
    public float raceTime;
    public bool raceStarted;
    public bool raceFinished;
    Text hudText, messageText;
    GameObject hudCanvas;
    readonly List<Transform> opponents = new List<Transform>();
    readonly List<float> opponentSpeeds = new List<float>();

    void Awake() { Instance = this; }
    void Start()
    {
        BuildWorld();
        BuildUI();
        StartCoroutine(Countdown());
    }

    void Update()
    {
        if (raceFinished && Input.GetKeyDown(KeyCode.R)) { UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); }
        if (raceStarted && !raceFinished) raceTime += Time.deltaTime;
        if (player != null && raceStarted && !raceFinished)
        {
            float throttle = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f;
            float brake = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f;
            speed = Mathf.MoveTowards(speed, throttle > 0 ? maxSpeed : (brake > 0 ? -8f : 0f), (throttle > 0 ? 22f : 28f) * Time.deltaTime);
            float steer = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) steer = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steer = 1;
            player.position += player.forward * speed * Time.deltaTime;
            player.position += Vector3.right * steer * Mathf.Clamp(Mathf.Abs(speed) / 18f, 0.25f, 1.8f) * 10f * Time.deltaTime;
            player.position = new Vector3(Mathf.Clamp(player.position.x, -roadWidth * .48f, roadWidth * .48f), 0.65f, player.position.z);
            player.rotation = Quaternion.Euler(0, steer * 9f, 0);
            if (player.position.z >= roadLength) { player.position = new Vector3(player.position.x, .65f, 0); laps++; if (laps >= totalLaps) FinishRace(); }
            FollowCamera();
        }
        MoveOpponents();
        if (hudText != null) hudText.text = "SPEED  " + Mathf.RoundToInt(Mathf.Abs(speed) * 3.6f) + " KM/H     LAP  " + Mathf.Min(laps + 1, totalLaps) + "/" + totalLaps + "     TIME  " + raceTime.ToString("0.0") + "s";
    }

    void BuildWorld()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientLight = new Color(.72f, .61f, .45f);
        RenderSettings.fog = true; RenderSettings.fogColor = new Color(.88f,.66f,.40f); RenderSettings.fogMode = FogMode.Exponential; RenderSettings.fogDensity = .0018f;
        GameObject sun = new GameObject("Desert Sun"); Light sl = sun.AddComponent<Light>(); sl.type = LightType.Directional; sl.intensity = 1.35f; sun.transform.rotation = Quaternion.Euler(48,-32,0);
        Material sand = Mat(new Color(.78f,.56f,.30f)); Material road = Mat(new Color(.16f,.17f,.18f)); Material line = Mat(new Color(.94f,.86f,.64f)); Material rock = Mat(new Color(.42f,.27f,.17f)); Material cactus = Mat(new Color(.10f,.30f,.13f)); Material tire = Mat(new Color(.025f,.025f,.025f));
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name="Endless Desert Floor"; ground.transform.position=new Vector3(0,-.12f,roadLength/2); ground.transform.localScale=new Vector3(150,1,120); ground.GetComponent<Renderer>().material=sand;
        // rolling dunes
        for(int i=0;i<34;i++){ float z=(i*31)%((int)roadLength); float side=(i%2==0?-1:1); GameObject dune=GameObject.CreatePrimitive(PrimitiveType.Sphere); dune.name="Sand Dune"; dune.transform.position=new Vector3(side*(24+(i%5)*8),-1.8f,z); dune.transform.localScale=new Vector3(35+(i%4)*8,5+(i%3)*2,48); dune.GetComponent<Renderer>().material=sand; }
        GameObject track=GameObject.CreatePrimitive(PrimitiveType.Cube); track.name="Main Asphalt Racing Road"; track.transform.position=new Vector3(0,0,roadLength/2); track.transform.localScale=new Vector3(roadWidth,.25f,roadLength); track.GetComponent<Renderer>().material=road;
        for(int z=0;z<roadLength;z+=18){ Cube("Road Center Dash",new Vector3(0,.14f,z+6),new Vector3(.22f,.025f,8),line); Cube("Left Road Edge",new Vector3(-roadWidth/2+.35f,.14f,z+9),new Vector3(.18f,.025f,18),line); Cube("Right Road Edge",new Vector3(roadWidth/2-.35f,.14f,z+9),new Vector3(.18f,.025f,18),line); }
        // roadside props in varied clusters
        for(int z=12;z<roadLength;z+=13){ for(int s=-1;s<=1;s+=2){ float x=s*(13+Random.Range(0,16)); int kind=Random.Range(0,4); if(kind<2) MakeCactus(new Vector3(x,0,z+Random.Range(-4,4)),cactus); else if(kind==2) MakeRock(new Vector3(x,0,z),rock); else MakeDryShrub(new Vector3(x,0,z)); } }
        // start / finish gantry
        Cube("Start Finish Banner",new Vector3(0,5,8),new Vector3(roadWidth,1,.5f),Mat(new Color(.65f,.16f,.08f)));
        Cube("Start Finish Left Post",new Vector3(-roadWidth/2,2.5f,8),new Vector3(.5f,5,.5f),line); Cube("Start Finish Right Post",new Vector3(roadWidth/2,2.5f,8),new Vector3(.5f,5,.5f),line);
        CreateCar("PLAYER CAR",new Vector3(0,.65f,2),new Color(.05f,.35f,.9f),true);
        for(int i=0;i<5;i++){ float x=(i%2==0?-4.5f:4.5f); float z=24+i*13; CreateCar("RIVAL " +(i+1),new Vector3(x,.65f,z),new Color(Random.Range(.55f,1f),Random.Range(.08f,.35f),Random.Range(.03f,.15f)),false); }
        Camera cam=Camera.main; if(cam==null){GameObject c=new GameObject("Main Camera"); cam=c.AddComponent<Camera>(); c.tag="MainCamera";} cam.fieldOfView=68; cam.transform.position=new Vector3(0,6,-10); cam.transform.rotation=Quaternion.Euler(18,0,0); cam.backgroundColor=new Color(.78f,.55f,.32f);
        FollowCamera();
    }

    Material Mat(Color c){Shader shader=Shader.Find("Standard"); if(shader==null) shader=Shader.Find("Universal Render Pipeline/Lit"); Material m=new Material(shader); m.color=c; return m;}
    void Cube(string n,Vector3 p,Vector3 sc,Material m){GameObject o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.position=p;o.transform.localScale=sc;o.GetComponent<Renderer>().material=m;}
    void MakeRock(Vector3 p,Material m){GameObject o=GameObject.CreatePrimitive(PrimitiveType.Sphere);o.name="Desert Rock";o.transform.position=p+Vector3.up*.45f;o.transform.localScale=new Vector3(Random.Range(1f,2.5f),Random.Range(.6f,1.5f),Random.Range(1f,2.2f));o.transform.rotation=Random.rotation;o.GetComponent<Renderer>().material=m;}
    void MakeCactus(Vector3 p,Material m){GameObject trunk=GameObject.CreatePrimitive(PrimitiveType.Capsule);trunk.name="Saguaro Cactus";trunk.transform.position=p+Vector3.up*1.35f;trunk.transform.localScale=new Vector3(.55f,1.4f,.55f);trunk.GetComponent<Renderer>().material=m; GameObject arm=GameObject.CreatePrimitive(PrimitiveType.Capsule);arm.transform.position=p+new Vector3(.48f,1.65f,0);arm.transform.localScale=new Vector3(.22f,.75f,.22f);arm.GetComponent<Renderer>().material=m; GameObject arm2=GameObject.CreatePrimitive(PrimitiveType.Capsule);arm2.transform.position=p+new Vector3(.25f,2.05f,0);arm2.transform.localScale=new Vector3(.22f,.22f,.22f);arm2.GetComponent<Renderer>().material=m;}
    void MakeDryShrub(Vector3 p){Material m=Mat(new Color(.42f,.27f,.10f));for(int i=0;i<5;i++){GameObject b=GameObject.CreatePrimitive(PrimitiveType.Cylinder);b.name="Dry Desert Bush";b.transform.position=p+Vector3.up*.45f;b.transform.localScale=new Vector3(.12f,.65f,.12f);b.transform.rotation=Quaternion.Euler(Random.Range(-40,40),Random.Range(0,360),Random.Range(-40,40));b.GetComponent<Renderer>().material=m;}}
    void CreateCar(string n,Vector3 p,Color color,bool isPlayer){GameObject root=new GameObject(n);root.transform.position=p; GameObject body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.name="Car Body";body.transform.SetParent(root.transform);body.transform.localPosition=new Vector3(0,.45f,0);body.transform.localScale=new Vector3(2.2f,.65f,4);body.GetComponent<Renderer>().material=Mat(color); GameObject cabin=GameObject.CreatePrimitive(PrimitiveType.Cube);cabin.transform.SetParent(root.transform);cabin.transform.localPosition=new Vector3(0,1, -.25f);cabin.transform.localScale=new Vector3(1.45f,.65f,1.9f);cabin.GetComponent<Renderer>().material=Mat(new Color(.12f,.2f,.25f)); for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){GameObject w=GameObject.CreatePrimitive(PrimitiveType.Cylinder);w.transform.SetParent(root.transform);w.transform.localPosition=new Vector3(x*1.05f,.25f,z*1.25f);w.transform.localRotation=Quaternion.Euler(0,0,90);w.transform.localScale=new Vector3(.72f,.24f,.72f);w.GetComponent<Renderer>().material=Mat(new Color(.025f,.025f,.025f));} if(isPlayer){player=root.transform;}else{opponents.Add(root.transform);opponentSpeeds.Add(Random.Range(24f,39f));}}
    void MoveOpponents(){for(int i=0;i<opponents.Count;i++){Transform t=opponents[i];if(t==null)continue;t.position+=Vector3.forward*opponentSpeeds[i]*Time.deltaTime;if(t.position.z>roadLength)t.position=new Vector3(t.position.x,.65f,15+i*12);}}
    void FollowCamera(){if(player==null||Camera.main==null)return;Camera.main.transform.position=Vector3.Lerp(Camera.main.transform.position,player.position+new Vector3(0,6,-10),Time.deltaTime*5);Camera.main.transform.LookAt(player.position+Vector3.forward*8+Vector3.up*1.2f);}
    void BuildUI(){hudCanvas=new GameObject("Race HUD");Canvas c=hudCanvas.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;hudCanvas.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;hudCanvas.AddComponent<GraphicRaycaster>();hudText=MakeText(c.transform,"HUD",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-35),28,TextAnchor.MiddleCenter);messageText=MakeText(c.transform,"Race Message",new Vector2(.5f,.55f),new Vector2(.5f,.55f),Vector2.zero,48,TextAnchor.MiddleCenter);messageText.text="DESERT RALLY\nGET READY";}
    Text MakeText(Transform parent,string n,Vector2 amin,Vector2 amax,Vector2 pos,int size,TextAnchor align){GameObject o=new GameObject(n);o.transform.SetParent(parent,false);RectTransform r=o.AddComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.anchoredPosition=pos;r.sizeDelta=new Vector2(1000,150);Text t=o.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.alignment=align;t.color=Color.white;t.fontStyle=FontStyle.Bold;return t;}
    IEnumerator Countdown(){for(int i=3;i>0;i--){messageText.text=i.ToString();yield return new WaitForSeconds(1);}messageText.text="GO!";raceStarted=true;yield return new WaitForSeconds(1.2f);messageText.text="";}
    void FinishRace(){raceFinished=true;speed=0;messageText.text="RACE COMPLETE!\nTIME: "+raceTime.ToString("0.0")+"s\nPress R to restart";}
}
