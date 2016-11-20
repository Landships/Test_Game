using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System;


public class PlayerController_VR : MonoBehaviour
{
    public byte owner;
    byte current_player;

    public GameObject camera_rig;
    public GameObject left_controller;
    public GameObject right_controller;

    public GameObject left_hand;
    public GameObject right_hand;

    // Client Queue
    int frame = 0;
    Queue<Vector3> past_left_positions;
    Queue<Vector3> past_right_positions;

    // Lerping
    bool left_lerping = false;
    bool right_lerping = false;
    float lerp_time = 1.0f;
    float current_left_lerp_time;
    float current_right_lerp_time;
    Vector3 lerp_final_left_position;
    Vector3 lerp_final_right_position;

    //Client to send
    byte[] client_info = new byte[12];
    float[] client_cache = new float[3];


    int server_player;

    // general
    float left_x;
    float left_y;
    float left_z;

    float right_x;
    float right_y;
    float right_z;

    float server_left_x;
    float server_left_y;
    float server_left_z;

    float server_right_x;
    float server_right_y;
    float server_right_z;

    float server_left_angle_x;
    float server_left_angle_y;
    float server_left_angle_z;

    float server_right_angle_x;
    float server_right_angle_y;
    float server_right_angle_z;

    //trigger

    GameObject n_manager;
    network_manager n_manager_script;

    bool started = false;
    bool ready = false;


    void Start()
    {
        n_manager = GameObject.Find("Custom Network Manager(Clone)");
        n_manager_script = n_manager.GetComponent<network_manager>();
        current_player = (byte)(n_manager_script.client_players_amount);
        //client_update_world(n_manager_script.server_to_client_data_large);
        //server_get_data_to_send();

        past_left_positions = new Queue<Vector3>(10);
        past_right_positions = new Queue<Vector3>(10);

    }

    void Update()
    {
        //DEBUG
        //Buffer.BlockCopy(n_manager_script.server_to_client_data_large, 0, data_cache, 3, );
        

        //client_get_data_to_send();
        started = n_manager_script.started;
        ready = n_manager_script.game_ready;

        server_player = n_manager_script.server_player_control;

        if (current_player == 1)
        {
            //Debug.Log("job for the server");
            // Server Updates world based off a clients inputs
            //server_update_world(n_manager_script.server_to_client_data);
            server_get_data_to_send();
        }

        ///Update position for owner and non-owner - client prediction
        update_client_values();

        //
        if (current_player != 1)
        {

            // Client updates its world based off the large server message
            if (started)
            {
                if (frame == 0)
                {
                    //client_update_world();

                }
                if (frame == 10)
                {
                    frame = -1;
                }
                frame++;
            }
            if (current_player != owner)
            {
                if (left_lerping == true)
                {
                    lerp_player_left_position();
                }
                if (right_lerping == true)
                {
                    lerp_player_right_position();
                }
                else
                {
                    //client_update_world();
                }
            }
        }
        Debug.Log("PLAYER");
        Debug.Log(current_player.ToString());
    }

    //if not owner and not host, do nothing, else:
    void update_client_values()
    {
        // Server move player or self
        if (current_player != owner)
        {
            server_player = n_manager_script.server_player_control;
            //host
            if (server_player == owner && camera_rig != null)
            {
                Read_Camera_Rig();
            }
        }

        // Client get inputs
        if (current_player == owner)
        {
            if (current_player == 1 && camera_rig != null)  // Current Player is the owner and the server
            {
                Read_Camera_Rig();
            }
            else
            {
                // Update the Queue with the current position we just enter

                past_left_positions.Enqueue(left_hand.transform.position);
                past_right_positions.Enqueue(right_hand.transform.position);

                //client_send_values();
            }
        }
    }


    void Read_Camera_Rig()
    {
        left_hand.transform.position = left_controller.transform.position;
        right_hand.transform.position = right_controller.transform.position;


    }


    void lerp_player_left_position()
    {
        current_left_lerp_time += Time.deltaTime;
        if (current_left_lerp_time > lerp_time)
        {
            left_lerping = false;
            current_left_lerp_time = lerp_time;
        }
        float percent = current_left_lerp_time / lerp_time;
        left_hand.transform.position = Vector3.Lerp(left_hand.transform.position, lerp_final_left_position, percent);
    }

    void lerp_player_right_position()
    {
        current_right_lerp_time += Time.deltaTime;
        if (current_right_lerp_time > lerp_time)
        {
            right_lerping = false;
            current_right_lerp_time = lerp_time;
        }
        float percent = current_right_lerp_time / lerp_time;
        right_hand.transform.position = Vector3.Lerp(right_hand.transform.position, lerp_final_right_position, percent);
    }




    public void server_get_data_to_send()
    {

        float[] data_cache = new float[24];
        byte one = n_manager_script.server_to_client_data_large[0];
        byte two = n_manager_script.server_to_client_data_large[1];
        byte three = n_manager_script.server_to_client_data_large[2];

        Buffer.BlockCopy(n_manager_script.server_to_client_data_large, 3, data_cache, 0, 96);

        int offset = 6;
        int index = 0;
        if (owner == 2)
        {
            index = index + offset;
        }
        if (owner == 3)
        {
            index = index + offset + offset;
        }
        if (owner == 4)
        {
            index = index + offset + offset + offset;
        }

        data_cache[index] = left_hand.transform.position.x;
        data_cache[index + 1] = left_hand.transform.position.y;
        data_cache[index + 2] = left_hand.transform.position.z;
        data_cache[index + 3] = right_hand.transform.position.x;
        data_cache[index + 4] = right_hand.transform.position.y;
        data_cache[index + 5] = right_hand.transform.position.z;

        byte[] data_out = new byte[99];
        Buffer.BlockCopy(data_cache, 0, data_out, 3, 96);
        data_out[0] = one;
        data_out[1] = two;
        data_out[2] = three;

        //Buffer.BlockCopy(data_out, 0, n_manager_script.server_to_client_data_large, 0, 115);
        //Debug.Log("Server should be here");
        n_manager_script.server_to_client_data_large = data_out;






    }

    public byte get_client_player_number()
    {
        return current_player;
    }




}