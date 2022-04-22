<?php
function getData($method)
{
    $data = new stdClass();

    if ($method !== "GET") {
        $data->body = json_decode(file_get_contents('php://input'));
    }

    $data->parameters = [];
    $dataGet = $_GET;
    foreach ($dataGet as $key => $value) {
        if ($key !== "q") {
            $data->parameters[$key] = $value;
        }
    }

    return $data;
}

function getMethod()
{
    return $_SERVER['REQUEST_METHOD'];
}

header("content-type: application/json");
require_once "connectingToDB.php";

$url = isset($_GET['q']) ? $_GET['q'] : '';
$url = rtrim($url, '/');
$urlList = explode('/', $url);

$controller = $urlList[0];
$requestData = getData(getMethod());

if (file_exists(realpath(dirname(__FILE__)) . '/Controllers/' . $controller . '.php')) {
    include_once 'Controllers/' . $controller . '.php';
    route(getMethod(), $urlList, $requestData);
} else {
    echo $controller;
}
?>