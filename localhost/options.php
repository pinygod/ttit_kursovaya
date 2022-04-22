<?php
require_once "connectingToDB.php";


$codes = new stdClass();
$codes->_200 = "OK";
$codes->_400 = "Bad request. If some data are strange";
$codes->_403 = "Permission denied. Authorization token are invalid";
$codes->_500 = "Unexpected error";


function generateToken(): string
{
    return bin2hex(random_bytes(20));
}

function PrintJson($query_res)
{
    if (isset($query_res)) {
        if ($query_res->num_rows > 0) {
            $array = array();
            while ($row = $query_res->fetch_assoc()) {
                array_push($array, $row);
            }
            echo json_encode($array);
        } else {
            echo "The request is incorrect or there is no such data";
        }
    }
}

function GetRole($token)
{
    global $connect;

    $roles = GetListOfUsersRole();
    if (isset($token)) {
        $token = str_replace("Bearer ", "", $token);
        $authorizedUser = mysqli_fetch_array($connect->query("SELECT * FROM `user` WHERE `user`.`token` = '$token'"));

        if (isset($authorizedUser)) {
            $roleId = $authorizedUser["roleId"];
            if ($roles->AdminRole == $roleId) {
                return $roles->AdminRole;
            } else if ($roles->UserRole === $roleId) {
                return $roles->UserRole;
            }

            return $roles->UserRole;
        } else {
            return $roles->Unauthorized;
        }
    }
}

function CheckAuth($headers, $roles): int
{
    if (isset($headers["Authorization"])) {
        return GetRole($headers["Authorization"]);
    } else {
        return $roles->Unauthorized;
    }
}

function GetListOfUsersRole(): stdClass
{
    $roles = new stdClass();
    $roles->AdminRole = 2;
    $roles->UserRole = 1;
    $roles->Unauthorized = 0;

    return $roles;
}

function UploadFile($taskId, $type): bool
{
    global $connect;

    if ($_FILES && $_FILES["file"]["error"] == UPLOAD_ERR_OK
        && ($_FILES['file']['type'] == 'text/plain')) {

        $name = htmlspecialchars(basename($_FILES["file"]["name"]));
        $path = "Uploads/Files/" . time() . $name;
        if (move_uploaded_file($_FILES["file"]["tmp_name"], $path)) {
            $request = "UPDATE `task` SET `$type` = '/$path' WHERE `task`.`taskId` = $taskId";
            return $connect->query($request);
        } else {
            return false;
        }
    } else {
        return false;
    }
}