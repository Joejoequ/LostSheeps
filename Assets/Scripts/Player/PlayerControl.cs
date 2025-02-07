using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Tilemaps;
using Proyecto26;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    //======================================== variables ============================================================
    // Dont Initiat the Value here plz.
    private Grid floorGrid; // Grid Map
    public GameObject plant;
    public GameObject turret;  // Plant Kind
    public GameObject radar;  // Plant Kind
    public GameObject horiBox;
    public GameObject vertBox;
    public HomeCanvas homeCanvas;
    public int seedNumber;
    public float horiInput;
    public float vertInput;
    public float horiDis;
    public float vertDis;
    public float horiSpeed;
    public float vertSpeed;
    public float dX;
    public float dY;
    public float currentHp;
    public float maxHp;
    public float speed;
    public string action;
    public bool isMoving;
    public string attackedBy;

    private ContactFilter2D filter; // Collider Detect Tools.
    private List<Collider2D> results;// Collider Detect Tools.
    
    public Vector3Int playerDirection; // 0 up; 1 up-right; 2 right; 3 down-right; 4 down; 5 down-left; 6 left; 7 up-left;
    public Animator anim;

    public CanvasManager canvasManager;
    public Dialogue dialogueUI;
    public Vector3Int playerGridPos;
    public Vector3Int targetrGridPos;
    public Vector3Int isMovingDirection;
    public Vector3 targetWorldPos;
    public SpriteRenderer spriteRenderer;
    public Image hpImage;
    public Sprite l;
    public Sprite r;
    public Sprite b;
    public Sprite f;
    private bool inputEnabled = true;
     private AudioSource playerAudio;
    [SerializeField] private AudioClip pushSound;
    [SerializeField] private AudioClip pickSound;
    [SerializeField] private AudioClip buildSound;
    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
    }

    //=============================================================================================================
    // Start is called before the first frame update
    void Start()
    {      
        spriteRenderer = GetComponent<SpriteRenderer>();
        canvasManager = GameObject.Find("Canvas").GetComponent<CanvasManager>();
        isMoving = false;
        action = "None";
        //timeToMove=0.2f;
        //seedNumber=0; //initiate the peaNumber.
        homeCanvas=GameObject.Find("HomeCanvas").GetComponent<HomeCanvas>();
        if(GameObject.Find("DialogueCanvas"))
            dialogueUI = GameObject.Find("DialogueCanvas").transform.GetChild(0).GetComponent<Dialogue>();
        currentHp = maxHp;
        speed = 2.5f; //Remember to Set the Speed on Start 
        vertSpeed = 2.5f;  //Remember to Set the Speed on Start 
        horiSpeed = 2.5f;
        filter = new ContactFilter2D().NoFilter(); //initiate the Collider Detect Tools.
        results = new List<Collider2D>(); //initiate the Collider Detect Tools.
        l = Resources.Load<Sprite>("Pictures/Player/l");
        r = Resources.Load<Sprite>("Pictures/Player/r");
        b = Resources.Load<Sprite>("Pictures/Player/b");
        f = Resources.Load<Sprite>("Pictures/Player/f");
        turret = Resources.Load("Prefabs/Turret/Turret") as GameObject; //Load Plant pea
        radar = Resources.Load("Prefabs/Buff/Radar") as GameObject; //Load Plant cherry
        
        //HP=1000f; // Set HP 
        floorGrid = GameObject.Find("Grid").GetComponent<Grid>(); // initate the Map
        playerGridPos = floorGrid.WorldToCell(transform.position); //Find the Player position in GridSpace
        transform.position = floorGrid.GetCellCenterWorld(playerGridPos);
        playerDirection = new Vector3Int(0, -1, 0); //Set default direction
        dX = 0f; //initiate the direction
        dY = -1f; //initiate the direction
        targetrGridPos = playerGridPos + new Vector3Int(0, -1, 0); //initiate the target position in Grid Space
        targetWorldPos = floorGrid.GetCellCenterWorld(targetrGridPos); //initiate the target position in World Space
        playerAudio = GetComponent<AudioSource>();
        plant = turret;
        int level=-1;
        string name=SceneManager.GetActiveScene().name;
        if(name!="Home"){
            
            level=(int.Parse(name ));
        }
        if((homeCanvas.levels[3]!=1&&(homeCanvas.levels[4]==2&&level==4))){ //Whatif press the #2.
            plant = radar;

        }

    }
    //=============================================================================================================
    // Update is called once per frame
    void Update()
    {   
        // if dialogue UI exists and has not ended, diable player movement
        if(!canvasManager.ifStart || !inputEnabled || (dialogueUI && !dialogueUI.dialogueEnded)){
            return;
        }
        
        //HP UI
        hpImage.fillAmount = currentHp/maxHp;

        // Movement Input
        horiInput = Input.GetAxis("Horizontal");
        vertInput = Input.GetAxis("Vertical");

    //=============================================================================================================
    // Hp
        if(currentHp < 0){
            transform.eulerAngles = new Vector3(0, 0, 90f);
            canvasManager.ifRestart = true;
            PlayingStats.onLevelFail();
            PlayingStats.deathCount(attackedBy);
        }

    //=============================================================================================================
    // Facing direction;
        Vector3Int newDirection = Vector3Int.zero;
        if (horiInput > 0) 
        {   
            newDirection = Vector3Int.right;
        }
        else if(horiInput < 0)
        {
            newDirection = Vector3Int.left;
        }
        else if(vertInput>0)
        {
            newDirection = Vector3Int.up;
        }else if(vertInput<0){
            newDirection = Vector3Int.down;
        }

        if(newDirection != Vector3Int.zero){
            if(playerDirection != newDirection){
                playerDirection = newDirection;
                updateSprite();
            }
        }
        updateTarget();
        //=============================================================================================================
        // All Input setting are here, learn these code and expand these codes in the future.
        //=============================================================================================================
        updateAnim(horiInput,vertInput);

        playerGridPos = floorGrid.WorldToCell(transform.position); //Find the Player position in Grid Space

        
        if(!isMoving){
            if(horiInput!=0){
                isMoving=true;
                isMovingDirection = playerGridPos+new Vector3Int(horiInput>0 ? 1 : -1, 0, 0);
            }else if(vertInput!=0){
                isMovingDirection = playerGridPos+new Vector3Int(0, vertInput>0 ? 1 : -1, 0);
                isMoving=true;
            }

            action="None";
        }else{
            // Calculate the movement vector based on input and speed
            Vector3 movingDirection = new Vector3(horiInput, vertInput, 0);
            if(movingDirection.magnitude > 1)
                movingDirection.Normalize(); // moving speed won't exceed 1

            Vector3 movement = movingDirection * speed * Time.deltaTime;
            // Update transform position
            if(movement.magnitude>0.1f){
                
            }
            transform.position += movement;
        }
        
        //=============================================================================================================
        // Other Input
        if(Input.GetKeyDown(KeyCode.Space)){  // Whatif press the space.
            
            Physics2D.OverlapCircle(targetWorldPos, 0.1f,filter, results);
            foreach( Collider2D result in results)
            {
                if(result.gameObject.TryGetComponent<Box>(out Box box)){
                    playerAudio.PlayOneShot(pushSound, 1.0f);
                    box.direction=playerDirection;
                    box.action="move";
                    // box.setTargeted(false);
                }
                // if(result.gameObject.TryGetComponent<Wall>(out Wall wall)){
                //     wall.direction=playerDirection;
                //     wall.action="move";
                // }
            }
        }
        if(Input.GetKeyDown(KeyCode.E)){ // Whatif press the E.
            Physics2D.OverlapCircle(targetWorldPos, 0.1f,filter, results);
            foreach( Collider2D result in results)
            {
                if(result.gameObject.TryGetComponent<Box>(out Box box)){
                    

                    if(box.transform.childCount==0){
                        if(seedNumber==0){
                            GameObject.Find("Canvas").GetComponent<CanvasManager>().componentCounterText.GetComponent<TwinkleText>().Twinkle();
                        }
                        else{
                            if(plant==turret && seedNumber>0){
                                playerAudio.PlayOneShot(buildSound, 0.1f);
                                GameObject obj=Instantiate(plant, result.gameObject.transform.position-new Vector3(0f,0.001f,0f),Quaternion.identity,result.gameObject.transform);
                                seedNumber-=1;
                                PlayingStats.plantCount(obj.GetComponent<AttackTurret>().GetType().Name);
                                box.CheckNeighbors();
                                
                            }
                            if(plant==radar && seedNumber>0){
                                playerAudio.PlayOneShot(buildSound, 0.1f);
                                GameObject obj=Instantiate(plant, result.gameObject.transform.position-new Vector3(0f,0.001f,0f),Quaternion.identity,result.gameObject.transform);
                                seedNumber-=1;
                                PlayingStats.plantCount(obj.GetComponent<BuffTurret>().GetType().Name);
                                box.CheckNeighbors();
                                
                            }
                        }
                    }
                }
            }
        }
        int level=-1;
        string name=SceneManager.GetActiveScene().name;
        if(name!="Home"){
            
            level=(int.Parse(name ));
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha1)){  //Whatif press the #1.
            plant = turret;
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)&&(homeCanvas.levels[4]==1||(homeCanvas.levels[4]==2&&level==4))){ //Whatif press the #2.
            plant = radar;

        }
        //=============================================================================================================
        // Collider for detect seeds.
        //=============================================================================================================
        Physics2D.OverlapCircle(transform.position, 0.2f,filter, results);
        foreach (Collider2D result in results)
        {
            if (result.gameObject.TryGetComponent<Seed>(out Seed seed))
            {   
                playerAudio.PlayOneShot(pickSound, 0.3f);
                seedNumber += 1;
                
                PlayingStats.pickCount(seed.GetType().Name);
                Destroy(result.gameObject);
            }
        }
        //     if(result.gameObject.TryGetComponent<Seed>(out Seed cherrySeed)){
        //         seedNumber+=1;
        //         Destroy(result.gameObject);
        //     }
        // }
    }
    private void SlowMove(Vector3 targetPos)
    {
       
        //Vector3Int targetCellPos = playerGridPos + direction;
        //Vector3 targetPos = floorGrid.GetCellCenterWorld(targetCellPos);
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCircle(targetPos, 0.1f,filter,results);
        bool isOccupied=false;
        foreach( Collider2D result in results)
        {   
            if(result.isTrigger){
                continue;
            }
            if(result.gameObject.TryGetComponent<Box>(out Box box)){
                isOccupied=true;
                break;
            }else if(result.gameObject.TryGetComponent<Enemy>(out Enemy enemy)){
                isOccupied=true;
                break;
            }else if(result.gameObject.TryGetComponent<Wall>(out Wall wall)){
                isOccupied=true;
                break;
            }             
        }
        if(!isOccupied){
            
            float elapsedTime = Time.deltaTime;
            Vector3 origPos = transform.position;
            // while(elapsedTime < timeToMove){
            //     transform.position = Vector3.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
            //     elapsedTime += Time.deltaTime;
            //     yield return null;
            // }
            transform.position=Vector3.MoveTowards(transform.position,targetPos,speed*Time.deltaTime);            
            //transform.position = targetPos;
            //playerGridPos = targetCellPos;
            if(Vector3.Distance(transform.position,targetPos)<0.01){
                isMoving = false;    
                
            }

        }else{
            isMoving = false;
            
        }    
        
        
    }
    private GameObject GetBox(Vector3 position){
        Physics2D.OverlapCircle(position, 0.1f,filter, results);
        foreach( Collider2D result in results)
        {
            if(result.gameObject.TryGetComponent<Box>(out Box box)){
                return result.gameObject;
            }
        }
        return null;
    }

    private void updateSprite(){
/*        if(playerDirection == Vector3Int.left){
            spriteRenderer.sprite=l;
        } else if(playerDirection == Vector3Int.right){
            *//*spriteRenderer.sprite=r;*//*
            anim.SetFloat("Horizontal", 1);
        } else if(playerDirection == Vector3Int.up){
            spriteRenderer.sprite=f;
        } else if(playerDirection == Vector3Int.down){
            spriteRenderer.sprite=b;
        }*/
    }

    private void updateTarget(){
        if(targetrGridPos != playerGridPos + playerDirection){
            untargetBox();
            targetrGridPos = playerGridPos + playerDirection;
            targetWorldPos = floorGrid.GetCellCenterWorld(targetrGridPos);
            targetBox();
        }
    }

    private void targetBox(){
        Physics2D.OverlapCircle(targetWorldPos, 0.1f,filter, results);
        foreach( Collider2D result in results)
        {
            if(result.gameObject.TryGetComponent<Box>(out Box box)){
                box.setTargeted(true);
            }
        }
    }

    private void untargetBox(){
        Physics2D.OverlapCircle(targetWorldPos, 0.1f,filter, results);
        foreach( Collider2D result in results)
        {
            if(result.gameObject.TryGetComponent<Box>(out Box box)){
                box.setTargeted(false);
            }
        }
    }

    void updateAnim(float hori, float vert) {
        anim.SetFloat("Horizontal", hori);
        anim.SetFloat("Vertical", vert);

    }
}
