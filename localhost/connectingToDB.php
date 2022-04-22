<?php
    $connect = new mysqli("localhost", "mysql", "mysql", 'codehedgehog');
    if ($connect->connect_error)
    {
        echo '<br>' . '<b>Server Log</b> ' . '<br>';
        echo 'Error number: ' . $connect->connect_errno . '<br>';
        echo $connect->connect_error . '<br>';
        http_response_code(504);
        return;
    }

