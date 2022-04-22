<?php

require_once "connectingToDB.php";
require_once "options.php";

function route($method, $urlList, $requestData)
{
    switch ($method) {
        case "POST":
        {
            Post($requestData);
            break;
        }
        default:
            http_response_code(500);
            global $codes;
            echo($codes->_500);
            break;
    }
}

function Post($data)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);

    $token = str_replace("Bearer ", "", $header["Authorization"]);

    if ($token == "") {
        $name = $data->body->name;
        $surname = $data->body->surname;
        $username = $data->body->username;
        $password = $data->body->password;
        $token = generateToken();

        CheckForUniqueUsername($username);

        $connect->query("INSERT INTO `user` (`userId`, `name`, `surname`, `password`, `username`, `token`, `roleId`)
                    VALUES (NULL, '$name', '$surname', '$password', '$username', '$token', 1)");

        echo json_encode(array("token" => $token), JSON_FORCE_OBJECT);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function CheckForUniqueUsername($username)
{
    global $connect;

    $user = $connect->query("SELECT * FROM `user` WHERE `user`.`username` = '$username'");
    if ($user->num_rows != 0) {

        http_response_code(409);
        echo json_encode('message: Username already used');
        exit();
    }
}