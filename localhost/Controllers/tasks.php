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
    if (sizeof($url) == 2 && is_numeric($url[1])) {
        GetDetailedDataForSpecifiedTask($url);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "input") {
        GetInputForConcreteTask($url[1]); //??
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "output") {
        GetOutputForConcreteTask($url[1]); //??
    } else if (sizeof($url) != '') {
        GetListOfTasksInSystem($url, $body);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function GetInputForConcreteTask($taskId)
{
    global $connect;

    $inputPath = $connect->query("SELECT `task`.`input` FROM task WHERE taskId='$taskId'")->fetch_array()[0];
    if ($inputPath != "") {
        readfile(getcwd() . $inputPath);
        die();
    } else {
        global $codes;
        echo($codes->_400);
    }
}

function GetOutputForConcreteTask($taskId)
{
    global $connect;

    $outputPath = $connect->query("SELECT `task`.`output` FROM task WHERE taskId='$taskId'")->fetch_array()[0];
    if ($outputPath != "") {
        readfile(getcwd() . $outputPath);
        die();
    } else {
        global $codes;
        echo($codes->_400);
    }
}

function GetDetailedDataForSpecifiedTask($url)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);

    $taskId = $url[1];
    $data = $connect->query("SELECT * FROM task WHERE taskId='$taskId'");



    PrintJson($data);
}

function GetListOfTasksInSystem($url, $body)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);

    $nameTask = $body->parameters['name'];
    $topicIdTask = $body->parameters['topicId'];

    if ($nameTask != '' && $topicIdTask != '') {
        $data = $connect->query("SELECT taskId, name, topicId FROM task WHERE name='$nameTask', topicId='$topicIdTask'");
    } else if ($nameTask != '' && $topicIdTask == '') {
        $data = $connect->query("SELECT taskId, name, topicId FROM task WHERE name='$nameTask'");
    } else if ($nameTask == '' && $topicIdTask != '') {
        $data = $connect->query("SELECT taskId, name, topicId FROM task WHERE topicId='$topicIdTask'");
    } else if ($nameTask == '' && $topicIdTask == '') {
        $data = $connect->query("SELECT taskId, name, topicId FROM task");
    } else {
        http_response_code(400);
        echo(json_encode(array("message" => "Something went wrong in method GET tasks/")));
    }

    unset($data->img);
    PrintJson($data);
}

function Post($url, $data)
{
    if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "input") {
        UploadConcreteTaskInputInSystem($url, $data);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "output") {
        UploadConcreteTaskOutputInSystem($url, $data);
    } else if (sizeof($url) == 1) {
        CreateNewTaskInSystem($url, $data);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "solution") {
        CreateNewSolution($url, $data);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function CreateNewSolution($url, $body){
    $headers = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($headers, $roles);

    $token = str_replace("Bearer ", "", $headers["Authorization"]);

    $taskId = $url[1];

    $sourceCode = $body->body->sourceCode;
    $programmingLanguage = $body->body->programmingLanguage;

    if($token != ""){
        $userId = mysqli_fetch_array($connect->query("SELECT `user`.`userId` FROM `user` WHERE `user`.`Token` = '$token'"))[0];
        if ($sourceCode != "" && $programmingLanguage != "") {
            $connect->query("INSERT INTO `solution` (`sourceCode`, `programmingLanguage`, `taskId`, `authorId`) VALUES ('$sourceCode', '$programmingLanguage', '$taskId', '$userId')");

            $lastId = $connect->insert_id;

            $solution = $connect->query("SELECT * FROM solution WHERE `solutionId` = '$lastId'");
            PrintJson($solution);
        }
        else{
            http_response_code(400);
            global $codes;
            echo($codes->_400);
        }
    }
    else{
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function CreateNewTaskInSystem($url, $data)
{
    $header = getallheaders();
    global $connect;

    $data = $data->body;
    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);

    if ($userRole == $roles->AdminRole) {
        $description = addslashes($data->description);
        $query = "INSERT INTO `task` (`name`, `topicId`, `description`, `price`) VALUES ('$data->name', '$data->topicId', '$description', '$data->price')";
        $res = $connect->query($query);

        $lastId = $connect->insert_id;
        $data = $connect->query("SELECT * FROM task WHERE taskId='$lastId'");
        unset($data->input);
        unset($data->output);

        PrintJson($data);

    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);;
    }
}

function UploadConcreteTaskInputInSystem($url, $data)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);
    $taskId = $url[1];

    if ($userRole == $roles->AdminRole) {
        $isUploaded = UploadFile($taskId, "input");

        if ($isUploaded) {
            $res = $connect->query("SELECT `task`.`taskId`, `task`.`name`, `task`.`topicId`, `task`.`description`, `task`.`price`, `task`.`isDraft` FROM task WHERE taskId='$taskId'");
            PrintJson($res);
        } else {
            http_response_code(500);
            echo(json_encode(array("message" => "Something went wrong in method _MethodName.")));
        }
    } else {
        http_response_code(403);
        echo(json_encode(array("message" => "Permission denied. Authorization token are invalid")));
    }
}

function UploadConcreteTaskOutputInSystem($url, $data)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);
    $taskId = $url[1];

    if ($userRole == $roles->AdminRole) {
        $isUploaded = UploadFile($taskId, "output");

        if ($isUploaded) {
            $res = $connect->query("SELECT `task`.`taskId`, `task`.`name`, `task`.`topicId`, `task`.`description`, `task`.`price`, `task`.`isDraft` FROM task WHERE taskId='$taskId'");
            PrintJson($res);
        } else {
            http_response_code(500);
            global $codes;
            echo($codes->_500);
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
        EditConcreteTaskInSystem($url, $body);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function EditConcreteTaskInSystem($url, $data)
{
    $header = getallheaders();
    global $connect;

    $data = $data->body;
    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);
    $taskId = $url[1];
    $description = addslashes($data->description);

    if ($userRole == $roles->AdminRole) {
        $connect->query("UPDATE task SET name='$data->name', topicId='$data->topicId', description='$description', price='$data->price' WHERE taskId='$taskId'");

        $data = $connect->query("SELECT * FROM task WHERE taskId='$taskId'");

        unset($data->input);
        unset($data->output);

        PrintJson($data);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function Delete($url)
{
    if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "input") {
        DeleteInputForConcreteTask($url[1]);
    } else if (sizeof($url) == 3 && is_numeric($url[1]) && $url[2] == "output") {
        DeleteOutputForConcreteTask($url[1]);
    } else if (sizeof($url) == 2 && is_numeric($url[1])) {
        DeleteOfCurrentTask($url);
    } else {
        http_response_code(400);
        global $codes;
        echo($codes->_400);
    }
}

function DeleteOfCurrentTask($url)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);
    $taskId = $url[1];

    if ($userRole == $roles->AdminRole) {
        $inputPath = $connect->query("SELECT `task`.`input` FROM task WHERE taskId='$taskId'")->fetch_array()[0];
        if ($inputPath != "") {
            unlink(getcwd() . $inputPath);
        }

        $outputPath = $connect->query("SELECT `task`.`output` FROM task WHERE taskId='$taskId'")->fetch_array()[0];
        if ($inputPath != "") {
            unlink(getcwd() . $outputPath);
        }

        $connect->query("DELETE FROM `task` WHERE taskId = $taskId");
        http_response_code(200);
        global $codes;
        echo($codes->_200);
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function DeleteInputForConcreteTask($taskId)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);

    if ($userRole == $roles->AdminRole) {
        $inputPath = $connect->query("SELECT `task`.`input` FROM task WHERE taskId='$taskId'")->fetch_array()[0];
        if ($inputPath != "") {
            unlink(getcwd() . $inputPath);
            $request = "UPDATE `task` SET `input` = '' WHERE `task`.`taskId` = $taskId";
            $connect->query($request);
        } else {
            global $codes;
            echo($codes->_400);
        }
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}

function DeleteOutputForConcreteTask($taskId)
{
    $header = getallheaders();
    global $connect;

    $roles = GetListOfUsersRole();
    $userRole = CheckAuth($header, $roles);

    if ($userRole == $roles->AdminRole) {
        $outputPath = $connect->query("SELECT `task`.`output` FROM task WHERE taskId='$taskId'")->fetch_array()[0];
        if ($outputPath != "") {
            unlink(getcwd() . $outputPath);
            $request = "UPDATE `task` SET `output` = '' WHERE `task`.`taskId` = $taskId";
            $connect->query($request);
        } else {
            global $codes;
            echo($codes->_400);
        }
    } else {
        http_response_code(403);
        global $codes;
        echo($codes->_403);
    }
}