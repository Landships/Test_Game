using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System;


public class PlayerController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public byte owner;
    byte current_player;

    // Client Queue
    int frame = 0;
    Queue<Vector3> past_positions;

    // Lerping
    bool lerping = false;
    float lerp_time = 1.0f;
    float current_lerp_time;
    Vector3 lerp_final_position;




    //Client to send
    byte[] client_info = new byte[12];
    float[] client_cache = new float[3];



    int server_player;

    // general
    float x;
    float y;
    float z;

    float data_x;
    float data_y;
    float data_z;
    float angle_x;
    float angle_y;
    float angle_z;
    float fired = 0;


    float horizontal_input = 0;
    float vertical_input = 0;
    float fired_input = 0;

    GameObject n_manager;
    network_manager n_manager_script;

    bool started = false;
    bool ready = false;


    void Start()
    {
        n_manager = GameObject.Find("Custom Network Manager(Clone)");
        n_manager_script = n_manager.GetComponent<network_manager>();
        current_player = (byte) (n_manager_script.client_players_amount );
        //client_update_world(n_manager_script.server_to_client_data_large);
        server_get_data_to_send();

        past_positions = new Queue<Vector3>(10);

    }

    void Update()
    {
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

        update_client_values();

        if (current_player != 1)
        {

            // Client updates its world based off the large server message
            if( started)
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
                if (lerping == true)
                {
                    lerp_player_position();
                }
                else
                {
                    client_update_world();
                }


            }




        }

        

        
    }



    // Lerping the player position

    void lerp_player_position()
    {

        current_lerp_time += Time.deltaTime;
        if (current_lerp_time > lerp_time)
        {
            lerping = false;
            current_lerp_time = lerp_time;
        }
        float percent = current_lerp_time / lerp_time;
        transform.position = Vector3.Lerp(transform.position, lerp_final_position, percent);



    }










    void Fire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 20;

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }


    void update_client_values()
    {
        // Server move player or self
        if (current_player != owner)
        {

            //Debug.Log("server");
            //Debug.Log("server_player: " + server_player.ToString());
            server_player = n_manager_script.server_player_control;
            if (server_player == owner)
            {
                //Debug.Log("job for the server Deeper");
                //Debug.Log(fired_input.ToString());
                x = horizontal_input * Time.deltaTime * 150.0f;
                z = vertical_input * Time.deltaTime * 3.0f;

                transform.Rotate(0, x, 0);
                transform.Translate(0, 0, z * 2);

                if (fired_input == 1)
                {
                    Fire();
                }
            }
        }

        // Client get inputs
        if (current_player == owner)
        {

            if (current_player == 1)  // Current Player is the owner and the server
            {
                //Debug.Log("The server should move its own");
                x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
                z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
                transform.Rotate(0, x, 0);
                transform.Translate(0, 0, z * 2);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Fire();
                }
            }
            else
            {

                x = Input.GetAxis("Horizontal");
                z = Input.GetAxis("Vertical");
                transform.Rotate(0, x * Time.deltaTime * 150.0f, 0);
                transform.Translate(0, 0, z * Time.deltaTime * 3.0f * 2);


                // Update the Queue with the current position we just enter

                past_positions.Enqueue(transform.position);
                


                if (Input.GetKeyDown(KeyCode.Space))
                {
                    fired = 1;
                    Fire();
                }
                else
                {
                    fired = 0;
                }

                client_send_values();
            }



        }







    }



    void client_send_values()
    {

        client_cache[0] = x;
        client_cache[1] = z;
        client_cache[2] = fired;
        Buffer.BlockCopy(client_cache, 0, client_info, 0, 12);

        n_manager_script.client_send_information(client_info);

    }




    public void server_update_world(byte[] client_inputs)
    {
        float[] back = new float[3];
        Buffer.BlockCopy(client_inputs, 0, back, 0, 12);
        //Debug.Log(back[0].ToString());
        //Debug.Log(back[1].ToString());
        //Debug.Log(back[2].ToString());



        horizontal_input = back[0];
        vertical_input = back[1];
        fired_input = back[2];


    }




    void client_update_world()
    {

        //byte[] client_new_world = n_manager_script.server_to_client_data_large;
        float[] data = new float[28];
        Buffer.BlockCopy(n_manager_script.server_to_client_data_large, 3, data, 0, 112);
        int offset = 7;
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

        data_x = data[index];
        data_y = data[index + 1];
        data_z = data[index + 2];
        angle_x = data[index + 3];
        angle_y = data[index + 4];
        angle_z = data[index + 5];
        fired = data[index + 6];



        // The client is going to make a decision whether the new x y z data it recieved from the server is one 
        // that it has seen before and if so keep on using client side inputs.
        // If it has never been in that position before then it must move back to that location


        bool found = false;
        while(past_positions.Count != 0 && found != true)
        {
            Vector3 past_position = past_positions.Dequeue();

            Vector3 server_postion = new Vector3(data_x, data_y, data_z);
            float server_sq_distance = Vector3.Distance(past_position, server_postion);
            if (server_sq_distance < .05)
            {

                found = true;
            }
        }
        Debug.Log(past_positions.Count.ToString());
        if (found == false)
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
            lerping = true;
            lerp_final_position = new Vector3(data_x, data_y, data_z);
            current_lerp_time = 0f;
        }



    }






    public void server_get_data_to_send()
    {

        float[] data_cache = new float[28];
        byte one = n_manager_script.server_to_client_data_large[0];
        byte two = n_manager_script.server_to_client_data_large[1];
        byte three = n_manager_script.server_to_client_data_large[2];

        Buffer.BlockCopy(n_manager_script.server_to_client_data_large, 3, data_cache, 0, 112);

        int offset = 7;
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

        data_cache[index] = transform.position.x;
        data_cache[index + 1] = transform.position.y;
        data_cache[index + 2] = transform.position.z;
        data_cache[index + 3] = transform.eulerAngles.x;
        data_cache[index + 4] = transform.eulerAngles.y;
        data_cache[index + 5] = transform.eulerAngles.z;
        data_cache[index + 6] = fired;

        byte[] data_out = new byte[115];
        Buffer.BlockCopy(data_cache, 0, data_out, 3, 112);
        data_out[0] = one;
        data_out[1] = two;
        data_out[2] = three;

        //Buffer.BlockCopy(data_out, 0, n_manager_script.server_to_client_data_large, 0, 115);
        //Debug.Log("Server should be here");
        n_manager_script.server_to_client_data_large = data_out;






    }






}