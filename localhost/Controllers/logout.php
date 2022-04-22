<?php

require_once "connectingToDB.php";
require_once "options.php";

function route($method, $urlList, $requestData)
{
    switch ($method) {
        case "POST":
        {
            Post();
            break;
        }
        default:
            http_response_code(500);
            global $codes;
            echo($codes->_500);
            break;
    }
}

function Post()
{
    $header = getallheaders();
    global $connect;

    $token = str_replace("Bearer ", "", $header["Authorization"]);

    if ($token !== "") {
        $connect->query("UPDATE `user` SET `token` = '' WHERE `user`.`token` = '$token'");
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}
