using UnityEngine;
using System.Collections;

public class spawner_manager : MonoBehaviour
{
    public GameObject prefab_to_spawn;
    public GameObject prefab_to_spawn_vr;
    static GameObject camera_rig;
    static GameObject left_controller;
    static GameObject right_controller;


    void Start()
    {
        camera_rig = GameObject.Find("[CameraRig]");
        left_controller = camera_rig.transform.FindChild("Controller (left)").gameObject;
        right_controller = camera_rig.transform.FindChild("Controller (right)").gameObject;


    }


    public void spawn_four_players(byte host, byte first_connected, byte second_connected, byte third_connected)
    {
        Debug.Log("I will spawn 4 players");
        spawn_player(1, host);
        spawn_player(2, first_connected);
        spawn_player(3, second_connected);
        spawn_player(4, third_connected);

        GameObject n_manager = GameObject.Find("Custom Network Manager(Clone)");
        network_manager n_manager_script = n_manager.GetComponent<network_manager>();
        n_manager_script.game_ready = true;


    }



    void spawn_player(byte number, byte owner)
    {
        float x = 0;
        float y = 0;
        float z = 0;


        switch (number)
        {
            case 1:
                x = -15;
                y = 1;
                z = 15;

                break;

            case 2:
                x = 15;
                y = 1;
                z = 15;

                break;

            case 3:
                x = -15;
                y = 1;
                z = -15;

                break;

            case 4:
                x = 15;
                y = 1;
                z = -15;

                break;
        }


        // Instiantiate VR Players



        GameObject vr_player = Instantiate(prefab_to_spawn_vr, new Vector3(x, y, z), Quaternion.identity) as GameObject;

        vr_player.gameObject.GetComponent<PlayerController_VR>().owner = owner;

        Debug.Log("DONE");

        GameObject n_manager = GameObject.Find("Custom Network Manager(Clone)");
        network_manager n_manager_script = n_manager.GetComponent<network_manager>();
        byte current_player = (byte)(n_manager_script.client_players_amount);


        // ADD OWNER TODO!!!!!!!!!!!!!!!!!!
        Debug.Log("current player");
        Debug.Log(current_player);
        if (current_player == owner)
        {
            camera_rig.transform.position = new Vector3(x, y, z);
            vr_player.gameObject.GetComponent<PlayerController_VR>().camera_rig = camera_rig;

            //vr_player.gameObject.GetComponent<PlayerController_VR>().left_controller.transform.SetParent(camera_rig.transform.GetChild(0));
            // vr_player.gameObject.GetComponent<PlayerController_VR>().right_controller.transform.SetParent(camera_rig.transform.GetChild(1));
            vr_player.gameObject.GetComponent<PlayerController_VR>().left_controller = left_controller;
            vr_player.gameObject.GetComponent<PlayerController_VR>().right_controller = right_controller;
        }
        


    }


}
