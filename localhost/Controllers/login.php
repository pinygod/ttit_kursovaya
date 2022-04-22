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

    $password = $data->body->password;
    $username = $data->body->username;

    $token = str_replace("Bearer ", "", $header["Authorization"]);

    if ($token == "") {
        if ($password !== '' && $username !== '') {
            $token = generateToken();
            $connect->query("UPDATE `user` SET `token` = '$token' WHERE `user`.`username` = '$username' AND `user`.`password` = '$password'");

            echo json_encode(array("token" => $token), JSON_FORCE_OBJECT);
        } else {
            http_response_code(400);
            global $codes;
            echo($codes->_400);
        }
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}
