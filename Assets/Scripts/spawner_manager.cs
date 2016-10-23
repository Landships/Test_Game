using UnityEngine;
using System.Collections;

public class spawner_manager : MonoBehaviour
{
    public GameObject prefab_to_spawn;

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

        GameObject player = Instantiate(prefab_to_spawn, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        player.gameObject.GetComponent<PlayerController>().owner = owner;



        
    }


}
