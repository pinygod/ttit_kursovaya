<?php

require_once "connectingToDB.php";
require_once "options.php";

function route($method, $urlList, $requestData)
{
    switch ($method) {
        case "POST":
        {
            Post($urlList, $requestData);
            break;
        }

        case "GET":
        {
            Get($urlList);
            break;
        }

        case "PATCH":
        {
            Patch($urlList, $requestData);
            break;
        }

        case "DELETE":
        {
            Delete($urlList);
            break;
        }

        default:
            http_response_code(500);
            global $codes;
            echo($codes->_500);
            break;
    }
}

function Post($url, $body)
{
    if (sizeof($url) == 3 && $url[2] == 'role' && is_numeric($url[1])) {
        SetUsersRole($url, $body);

    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function SetUsersRole($url, $body)
{
    $headers = getallheaders();
    global $connect;

    $roleId = $body->body->roleId;

    $userId = $url[1];

    if (isset($roleId)) {
        $roles = GetListOfUsersRole();
        $userRole = CheckAuth($headers, $roles);

        if ($userRole == $roles->AdminRole) {

            if ($connect->query("UPDATE `user` SET `roleId` = $roleId WHERE userId = $userId")) {
                http_response_code(200);
                global $codes;
                echo($codes->_200);
            } else {
                http_response_code(500);
                global $codes;
                echo($codes->_500);
            }
        } else {
            http_response_code(403);
            global $codes;
            echo($codes->_403);
            exit();
        }
    } else {

        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function Get($url)
{
    if (sizeof($url) == 2 && is_numeric($url[1])) {
        GetDetailedDataForSpecifiedUser($url);
    } else if (sizeof($url) == 1) {
        GetListOfUsersInSystem();
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function GetDetailedDataForSpecifiedUser($url)
{
    $headers = getallheaders();
    global $connect;

    $userId = $url[1];

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);
    $token = str_replace("Bearer ", "", $headers["Authorization"]);
    $currentUserId = mysqli_fetch_array($connect->query("SELECT userId FROM `user` WHERE `user`.`token` = '$token'"))[0];

    if ($userRole == $roles->AdminRole || $userId == $currentUserId) {
        $users = $connect->query("SELECT userId, username, roleId, name, surname FROM user WHERE userId = $userId");
        PrintJson($users);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function GetListOfUsersInSystem()
{
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    if ($userRole == $roles->AdminRole) {
        $users = $connect->query("SELECT userId, username, roleId FROM user");
        PrintJson($users);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function Delete($url)
{
    $headers = getallheaders();
    global $connect;

    if (sizeof($url) == 2 && is_numeric($url[1])) {
        $roles = GetListOfUsersRole();
        $userRole = CheckAuth($headers, $roles);

        if ($userRole == $roles->AdminRole) {
            $userId = $url[1];
            $connect->query("DELETE FROM `user` WHERE userId = '$userId'");
            http_response_code(200);
            global $codes;
            echo($codes->_200);
        } else {
            http_response_code(403);
            global $codes;
            echo($codes->_403);
        }
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function Patch($url, $body)
{
    $headers = getallheaders();
    global $connect;

    $userId = $url[1];

    if (sizeof($url) == 2 && is_numeric($url[1])) {
        $roles = GetListOfUsersRole();
        $userRole = CheckAuth($headers, $roles);

        if ($userRole !== $roles->Unauthorized) {
            $body = $body->body;

            $password = $body->password;
            $name = $body->name;
            $surname = $body->surname;

            if ($password != '' && $name != '' && $surname != '') {
                $connect->query("UPDATE `user` SET password='$password', name='$name', surname='$surname' WHERE userId='$userId'");

                $user = $connect->query("SELECT userId, username, roleId, name, surname FROM user WHERE userId = $userId");

                unset($user->password);

                PrintJson($user);
                http_response_code(200);
                global $codes;
                echo($codes->_200);

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
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }

}



