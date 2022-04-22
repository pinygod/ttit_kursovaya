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
            Get($requestData->parameters["taskId"], $requestData->parameters["authorId"]);
            break;
        }

        default:
            http_response_code(500);
            global $codes;
            echo($codes->_500);
            break;
    }
}

function Get($task, $user)
{
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    if ($userRole == $roles->AdminRole) {

        echo($task);
        if ($task != 0 && $user != 0) {
            $users = $connect->query("SELECT * FROM solution WHERE `taskId` = '$task' AND `authorId` = '$user'");
            PrintJson($users);
        } else if ($task != 0) {
            $users = $connect->query("SELECT * FROM solution WHERE `taskId` = '$task'");
            PrintJson($users);
        } else if ($user != 0) {
            $users = $connect->query("SELECT * FROM solution WHERE `authorId` = '$user'");
            PrintJson($users);
        } else {
            $users = $connect->query("SELECT * FROM solution");
            PrintJson($users);
        }

    } else if ($userRole == $roles->UserRole) {
        $token = str_replace("Bearer ", "", $headers["Authorization"]);
        $userId = mysqli_fetch_array($connect->query("SELECT userId FROM `user` WHERE `user`.`token` = '$token'"))[0];

        if ($task != 0) {
            $users = $connect->query("SELECT * FROM solution WHERE `authorId`= '$userId' AND `taskId` = '$task'");
            PrintJson($users);
        } else {
            $users = $connect->query("SELECT * FROM solution WHERE `authorId` = '$userId'");
            PrintJson($users);
        }

    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function Post($url, $body)
{
    if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "postmoderation") {
        PostmoderateSolution($url, $body);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

const VERDICTS = array("Pending", "OK", "Rejected");

function PostmoderateSolution($url, $body)
{
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $verdict = $body->body->verdict;
    $solutionId = $url[1];

    if ($userRole == $roles->AdminRole) {
        if (in_array($verdict, VERDICTS, true)) {
            if ($connect->query("UPDATE `solution` SET `verdict` = '$verdict' WHERE solutionId = '$solutionId'")) {

                $taskId = $connect->query("SELECT `solution`.`taskId` FROM solution WHERE `solutionId` = '$solutionId'")->fetch_array()[0];
                $task = $connect->query("SELECT `task`.`taskId`, `task`.`name`, `task`.`topicId`, `task`.`description`, `task`.`price`, `task`.`isDraft` FROM task WHERE `taskId` = '$taskId'");

                PrintJson($task);
            } else {
                http_response_code(500);
                global $codes;
                echo($codes->_500);
            }
        } else {
            http_response_code(400);
            global $codes;
            echo($codes->_400);
        }
    }
    else{
        http_response_code(403);
        global $codes;
        $codes->_403;
    }
}


