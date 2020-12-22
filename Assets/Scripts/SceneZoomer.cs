using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneZoomer : MonoBehaviour
{

    public float zoom_speed = 5.0f;

    [Range(5.0f, 10f)]
    public float upper_zoom_bound = 8.0f;
    [Range(0.0f, 5.0f)]
    public float lower_zoom_bound = 1.0f;
    public float zoom_smooth_speed = 2.0f;
    
    [Range(0.0f, 1.0f)]
    public float camera_upper_xbound = 0.7f;
    [Range(0.0f, 1.0f)]
    public float camera_upper_ybound = 0.7f;
    [Range(0.0f, 1.0f)]
    public float camera_lower_xbound = 0.3f;
    [Range(0.0f, 1.0f)]
    public float camera_lower_ybound = 0.3f;
    public float camera_follow_lerp_step = 0.01f;
    public float camera_center_lerp_step = 0.01f;

    private float __target_ortho;
    private Camera __camera;
    private WorldManager __world_manager;

    private Tilemap __ground_tmap;

    // Start is called before the first frame update
    void Start()
    {
        __target_ortho = Camera.main.orthographicSize;
        __camera = GetComponent<Camera>();

        GameObject wm_GO = GameObject.Find("GameLoop");
        __world_manager =  (!!wm_GO) ? wm_GO.GetComponent<WorldManager>() : null;

        initTilemapDatas();

        checkBoundaries();

    }

    // expensive method
    private void initTilemapDatas()
    {
        var tmaps = FindObjectsOfType<Tilemap>();
        foreach( Tilemap t in tmaps )
        {
            if ( t.gameObject.layer == 9 ) // GROUND LAYER, dangerous put like this ?
            {    __ground_tmap = t; break;  }
        }
        if ( __ground_tmap!= null)
        {
            __ground_tmap.CompressBounds();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float scroll_zoom = Input.GetKey("p") ? 1f : 
                            Input.GetKey("o") ? -1f : 
                            0f ;

        if (scroll_zoom != 0f)
            zoom(scroll_zoom);
        checkBoundaries();

    }

    // TODO REMOVE HARDCODED RESOLUTION
    private bool isFullMapVisible()
    {
        if (__ground_tmap == null)
            return false;

        Bounds tmap_bounds = __ground_tmap.localBounds;

        Vector3 screenPos_minbound_vp = __camera.WorldToViewportPoint(tmap_bounds.min  + __ground_tmap.transform.position);
        Vector3 screenPos_maxbound_vp = __camera.WorldToViewportPoint(tmap_bounds.max + __ground_tmap.transform.position);

        bool isMapVisible = ( (screenPos_maxbound_vp.x < 1f ) &&
                              (screenPos_maxbound_vp.y < 1f ) &&
                              (screenPos_minbound_vp.x > 0f ) &&
                              (screenPos_minbound_vp.y > 0f ) );



        return isMapVisible;
    }

    private void tryCenter()
    {
        Debug.Log("try center");

        Bounds tmap_bounds = __ground_tmap.localBounds;

        Vector3 screenPos_minbound = __camera.WorldToScreenPoint( tmap_bounds.min  + __ground_tmap.transform.position );
        Vector3 screenPos_maxbound = __camera.WorldToScreenPoint( tmap_bounds.max + __ground_tmap.transform.position );

        // trouver le centre
        Vector3 center = Vector3.Lerp(tmap_bounds.min, tmap_bounds.max, 0.5f);
        center.z = __camera.transform.position.z;

        // aligner la cam dessus
        if ( __camera.transform.position != center )
            __camera.transform.position = Vector3.Lerp( __camera.transform.position, center, camera_center_lerp_step);
    }

    private void checkBoundaries()
    {
        // If tilemap not completelyt visible on screen (OR not centered?)
        if ( isFullMapVisible() )
        {
            //Debug.Log("map visible");
            tryCenter();
        } 
        else 
        {
            // If map is not completely visible in camera frame, then
            // lerp the positions
            GameObject player_go = __world_manager.Mdl.Players.Last();

            Vector3 screenPos = __camera.WorldToScreenPoint(player_go.transform.position);
            float xratio = screenPos.x / __camera.pixelWidth;
            float yratio = screenPos.y / __camera.pixelHeight;

            // xbound checks
            if ( ( xratio < camera_lower_xbound ) || ( xratio > camera_upper_xbound ) )
            {
                followPlayerInX(player_go.transform.position);
            }
            if ( ( yratio < camera_lower_ybound ) || ( yratio > camera_upper_ybound ) )
            {
                followPlayerInY(player_go.transform.position);
            }
        }
    }

    private void followPlayerInX( Vector3 target_position )
    {
        Vector3 lerp_target = new Vector3(target_position.x, __camera.transform.position.y, __camera.transform.position.z);
        __camera.transform.position = Vector3.Lerp(__camera.transform.position, lerp_target, camera_follow_lerp_step);
    }

    private void followPlayerInY( Vector3 target_position )
    {
        Vector3 lerp_target = new Vector3(__camera.transform.position.x, target_position.y, __camera.transform.position.z);
        __camera.transform.position = Vector3.Lerp(__camera.transform.position, lerp_target, camera_follow_lerp_step);
    }

    private void zoom( float iOffset )
    {
        __target_ortho -= iOffset * zoom_speed;
        __target_ortho = Mathf.Clamp(__target_ortho, lower_zoom_bound, upper_zoom_bound);
        Camera.main.orthographicSize = Mathf.MoveTowards (Camera.main.orthographicSize, __target_ortho, zoom_smooth_speed * Time.deltaTime);
    }
}
