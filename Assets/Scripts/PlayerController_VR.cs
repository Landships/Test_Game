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
    byte[] client_info = new byte[24];
    float[] client_cache = new float[6];


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
            server_update_world(n_manager_script.server_to_client_data);
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
                    client_update_world();

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
                    client_update_world();
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
                past_left_positions.Enqueue(left_hand.transform.position);
                past_right_positions.Enqueue(right_hand.transform.position);
            }
            if (camera_rig == null)
            {
                left_hand.transform.position = new Vector3(left_x, left_y, left_z);
                right_hand.transform.position = new Vector3(right_x, right_y, right_z);
            }
        }

        // Client get inputs
        if (current_player == owner)
        {
            if (camera_rig != null)  // Current Player is the owner and the server
            {
                Read_Camera_Rig();
                past_left_positions.Enqueue(left_hand.transform.position);
                past_right_positions.Enqueue(right_hand.transform.position);

                client_send_values();
            }

                // Update the Queue with the current position we just enter



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


    void client_send_values()
    {

        client_cache[0] = left_x;
        client_cache[1] = left_y;
        client_cache[2] = left_z;
        client_cache[3] = right_x;
        client_cache[4] = right_y;
        client_cache[5] = right_z;
        Buffer.BlockCopy(client_cache, 0, client_info, 0, 24);

        n_manager_script.client_send_information(client_info);

    }


    void client_update_world()
    {

        //byte[] client_new_world = n_manager_script.server_to_client_data_large;
        float[] data = new float[24];
        Buffer.BlockCopy(n_manager_script.server_to_client_data_large, 3, data, 0, 96);

        // 6 * 4byte things
        // 4byte things = left_handx, left_handy, left_handz, right_handx, right_handy, right_handz 
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


        left_x = data[index];
        left_y = data[index + 1];
        left_z = data[index + 2];
        right_x = data[index + 3];
        right_y = data[index + 4];
        right_z = data[index + 5];




        // The client is going to make a decision whether the new x y z data it recieved from the server is one 
        // that it has seen before and if so keep on using client side inputs.
        // If it has never been in that position before then it must move back to that location


        bool left_found = false;
        while (past_left_positions.Count != 0 && left_found != true)
        {
            Vector3 left_past_position = past_left_positions.Dequeue();

            Vector3 server_left_postion = new Vector3(left_x, left_y, left_z);
            float server_left_sq_distance = Vector3.Distance(left_past_position, server_left_postion);
            if (server_left_sq_distance < .05)
            {

                left_found = true;
            }
        }

        bool right_found = false;
        while (past_right_positions.Count != 0 && right_found != true)
        {
            Vector3 right_past_position = past_right_positions.Dequeue();

            Vector3 server_right_postion = new Vector3(right_x, right_y, right_z);
            float server_right_sq_distance = Vector3.Distance(right_past_position, server_right_postion);
            if (server_right_sq_distance < .05)
            {

                right_found = true;
            }
        }

        if (left_found == false)
        {
            /*
            transform.position = new Vector3(data_x, data_y, data_z);
            transform.rotation = Quaternion.Euler(angle_x, angle_y, angle_z);
            if (fired == 1)
            {
                Fire();
            }

            //Debug.Log("Player should be here");
            */
            left_lerping = true;
            lerp_final_left_position = new Vector3(left_x, left_y, left_z);
            current_left_lerp_time = 0f;
        }

        if (right_found == false)
        {
            /*
            transform.position = new Vector3(data_x, data_y, data_z);
            transform.rotation = Quaternion.Euler(angle_x, angle_y, angle_z);
            if (fired == 1)
            {
                Fire();
            }

            //Debug.Log("Player should be here");
            */
            right_lerping = true;
            lerp_final_right_position = new Vector3(right_x, right_y, right_z);
            current_right_lerp_time = 0f;
        }



    }








    public void server_update_world(byte[] client_inputs)
    {
        float[] back = new float[6];
        Buffer.BlockCopy(client_inputs, 0, back, 0, 24);
        //Debug.Log(back[0].ToString());
        //Debug.Log(back[1].ToString());
        //Debug.Log(back[2].ToString());



        left_x = back[0];
        left_y = back[1];
        left_z = back[2];

        right_x = back[3];
        right_y = back[4];
        right_z = back[5];


    }












}