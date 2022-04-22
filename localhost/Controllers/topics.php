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
            Get($urlList, $requestData);
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

function Get($url, $body)
{
    if (sizeof($url) == 1) {
        GetTopics($body->parameters["name"] ?? "", $body->parameters["parent"] ?? 0);
    } else if (sizeof($url) == 2 && is_numeric($url[1])) {
        GetTopic($url[1]);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "childs") {
        GetTopicChildren($url);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function GetTopicChildren($url)
{
    global $connect;

    $topicId = $url[1];
    $topics = $connect->query("SELECT * FROM `topic` WHERE parentId = '$topicId'");
    PrintJson($topics);
}

function GetTopic($topicId)
{
    global $connect;

    $topics = $connect->query("SELECT * FROM `topic` WHERE topicId = '$topicId'");
    $children = $connect->query("SELECT * FROM `topic` WHERE parentId = '$topicId'");

    $res = array();
    while ($row = $topics->fetch_assoc()) {
        $childrenArray = array();
        while ($child_row = $children->fetch_assoc()) {
            array_push($childrenArray, $child_row);
        }

        array_push($res, $row + array("childs" => $childrenArray));
    }

    echo(json_encode($res));
}

function GetTopics($name = "", $parent = 0)
{
    global $connect;

    if ($name != "" && $parent != 0) {
        $users = $connect->query("SELECT * FROM topic WHERE `name` LIKE '%$name%' AND `parentId` = '$parent'");
        PrintJson($users);
    } else if ($name != "") {
        $users = $connect->query("SELECT * FROM topic WHERE `name` LIKE '%$name%'");
        PrintJson($users);
    } else if ($parent != 0) {
        $users = $connect->query("SELECT * FROM topic WHERE `name` LIKE `parentId` = '$parent'");
        PrintJson($users);
    } else {
        $users = $connect->query("SELECT * FROM topic");
        PrintJson($users);
    }
}

function Post($url, $body)
{
    if (sizeof($url) == 1) {
        CreateTopic($url, $body);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "childs") {
        AddChildrenToCurrentTopic($url, $body);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function CreateTopic($url, $body)
{
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $body = $body->body;

    if ($userRole == $roles->AdminRole) {
        $name = $body->name;
        $parentId = $body->parentId;

        if (isset($parentId)) {
            $connect->query("INSERT INTO `topic` (`name`, `parentId`) VALUES ('$name', '$parentId')");
        } else {
            $connect->query("INSERT INTO `topic` (`name`) VALUES ('$name')");

            $parentId = $connect->insert_id;
        }

        GetTopic($parentId);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function AddChildrenToCurrentTopic($url, $body)
{
    $headers = getallheaders();
    global $connect;

    $topicId = $url[1];

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $body = $body->body;

    if ($userRole == $roles->AdminRole) {
        foreach ($body as $child) {
            $connect->query("UPDATE `topic` SET `parentId` = '$topicId ' WHERE id = '$child'");
        }
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function Patch($url, $body)
{
    if (sizeof($url) == 2 && is_numeric($url[1])) {
        EditTopic($url, $body);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function EditTopic($url, $body)
{
    $headers = getallheaders();
    global $connect;

    $topicId = $url[1];

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $body = $body->body;

    $name = $body->name;
    $parentId = $body->parentId;

    if ($userRole == $roles->AdminRole) {
        if (isset($name)) {
            $connect->query("UPDATE `topic` SET `name` = '$name' WHERE `topic`.`id` = '$topicId'");
        }
        if (isset($parentId)) {
            $connect->query("UPDATE `topic` SET `parentId` = '$parentId' WHERE `topic`.`id` = '$topicId'");
        }
        GetTopic($topicId);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function Delete($url, $body)
{
    global $connect;

    if (sizeof($url) == 1) {
        DeleteTopic($url);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "childs") {
        DeleteChildrenFromTopic($url, $body);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function DeleteTopic($url)
{
    $headers = getallheaders();
    global $connect;

    $topicId = $url[1];

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    if ($userRole == $roles->AdminRole) {
        $connect->query("DELETE FROM `topic` WHERE Id = '$topicId'");
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function DeleteChildrenFromTopic($url, $body){
    $headers = getallheaders();
    global $connect;

    $topicId = $url[1];

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $body = $body->body;

    if ($userRole == $roles->AdminRole) {
        foreach ($body as $child) {
            $connect->query("UPDATE `topic` SET `parentId` = null WHERE id = '$child'");
        }
        GetTopic($topicId);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}
