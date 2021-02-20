<?php
include("bl_Common.php");

$link = Connection::dbConnect();

$name = Utils::sanitaze_var($_POST['name'], $link);
$ip   = Utils::sanitaze_var($_POST['ip'], $link);
$typ  = Utils::sanitaze_var($_POST['typ'], $link);

if ($typ == 0) {//obsolete since ULogin 1.9
    $query = "SELECT * FROM " . BANS_DB . " ORDER by `id` DESC";
    $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());
    
    $num_results = mysqli_num_rows($result);
    if ($num_results > 0) {
        echo "result\n";
        for ($i = 0; $i < $num_results; $i++) {
            $row = mysqli_fetch_array($result);
            echo $row['name'] . "|" . $row['reason'] . "|" . $row['ip'] . "|" . $row['by'] . "|\n";
        }
    } else {
        echo "empty";
    }
} else if ($typ == "1") {//obsolete since ULogin 1.9
    $check2   = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE `ip`= '$ip' OR `name`= '$name'");
    $numrows2 = mysqli_num_rows($check2);
    if ($numrows2 != 0) {
        echo "yes";
    }
} else if ($typ == 2) {
    $result = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE ip='$ip'");
    if (mysqli_num_rows($result) != 0) {
        $row  = mysqli_fetch_array($result, MYSQLI_ASSOC);
        $data = json_encode($row, JSON_UNESCAPED_UNICODE);
        Response($data);
        http_response_code(202);
    } else {
        http_response_code(204);
        echo mysqli_num_rows($check2);
    }
}
else if ($typ == 3) {
    $check2   = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE `ip`= '$ip' OR `name`= '$name'");
    $numrows2 = mysqli_num_rows($check2);
    if ($numrows2 != 0) {
        http_response_code(302);
    }
}
mysqli_close($link);
?>