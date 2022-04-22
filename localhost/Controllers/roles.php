<?php

require_once "connectingToDB.php";
require_once "options.php";

function route($method, $urlList, $requestData)
{
    switch ($method) {
        case "GET":
        {
            Get($urlList);
            break;
        }
        default:
            http_response_code(500);
            global $codes;
            echo($codes->_500);
            break;
    }
}

function Get($url){
    if(sizeof($url) == 2 && is_numeric($url[1])){
        GetConcreteRole($url);
    }
    else if(sizeof($url) == 1){
        GetListOfAllRoles();
    }
    else{
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function GetListOfAllRoles()
{
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $token = str_replace("Bearer ", "", $headers["Authorization"]);

    if($token != ""){
        $roles = $connect->query("SELECT * FROM `role`");
        PrintJson($roles);
        exit();
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function GetConcreteRole($url)
{
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $roleId = $url[1];

    $token = str_replace("Bearer ", "", $headers["Authorization"]);

    if($token != ""){
        $role = $connect->query("SELECT * FROM `role` WHERE `roleId` = $roleId");
        PrintJson($role);
        exit();
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}
