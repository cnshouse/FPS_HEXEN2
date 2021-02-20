<?php
include("bl_Common.php");

$link = Connection::dbConnect();

$type        = Utils::sanitaze_var($_POST['type'], $link);
$clanID      = Utils::sanitaze_var($_POST['clanID'], $link);
$userID      = Utils::sanitaze_var($_POST['userID'], $link);
$description = Utils::sanitaze_var($_POST['desc'], $link);
$settings    = Utils::sanitaze_var($_POST['settings'], $link);
$hash        = safe($_POST['hash']);

$real_hash = md5(SECRET_KEY);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

if ($type == 99) //update clan score from a member
    {
    $query = "UPDATE Clans SET score=score+'$settings' WHERE id='$clanID'";
    if (mysqli_query($link, $query) or die(mysqli_error($link))) {
        echo "done";
    }
} else if ($type == 0) //get top 10 clans
    {
    $query = "SELECT * FROM `Clans` ORDER by `score` DESC LIMIT 10";
    $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());
    $num_results = mysqli_num_rows($result);
    if ($num_results > 0) {
        for ($i = 0; $i < $num_results; $i++) {
            $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
            echo $row['name'] . "|" . $row['score'] . "|" . $row['mcount'] . "|\n";
        }
    }
    mysqli_free_result($result);
} else if ($type == 1) //get clan info
    {
    $query = "SELECT * FROM Clans WHERE id='$clanID'";
    $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());
    $numrow = mysqli_num_rows($result);
    if ($numrow > 0) {
        $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
        echo "yes" . "|" . $row['name'] . "|" . $row['date'] . "|" . $row['members'] . "|" . $row['requests'] . "|" . $row['score'] . "|" . $row['settings'] . "|" . $row['description'] . "|";
        mysqli_free_result($result);
    } else {
        echo "Clan not exist.";
    }
} else if ($type == 2) //get clan members
    {
    $query = "SELECT * FROM " . PLAYERS_DB . " WHERE clan='$clanID'";
    $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());
    $numrow = mysqli_num_rows($result);
    if ($numrow > 0) {
        for ($i = 0; $i < $numrow; $i++) {
            $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
            echo $row['id'] . "|" . $row['nick'] . "-";
        }
        mysqli_free_result($result);
    } else {
        echo "Clan have not members.";
    }
} else if ($type == 3) // create clan
    {
    $query = "SELECT * FROM Clans WHERE `name`='$clanID'";
    $result = mysqli_query($link, $query) or die('check failed: ' . mysqli_error($link));
    $numsrow = mysqli_num_rows($result);
    if ($numsrow <= 0) {
        $scoreQuery = "SELECT score FROM " . PLAYERS_DB . " WHERE id='$userID'";
        $scoreResult = mysqli_query($link, $scoreQuery) or die(mysqli_error($link));
        $scoreRow = mysqli_fetch_array($scoreResult, MYSQLI_ASSOC);
        
        $fuser = $userID . "-3,"; //3 = the default leader ID
        date_default_timezone_set('America/Chicago');
        $date        = date('d/m/Y', time());
        $insertQuery = "INSERT INTO Clans (`name`,`description`,`members`,`mcount`,`settings`, `date`, `score`) VALUES ('" . $clanID . "','" . $description . "', '" . $fuser . "', 1, '" . $settings . "','" . $date . "','" . $scoreRow['score'] . "' )";
        $insertResult = mysqli_query($link, $insertQuery) or die('insert failed: ' . mysqli_error($link));
        if ($insertResult) {
            $last_ID     = mysqli_insert_id($link);
            $playerQuery = "UPDATE " . PLAYERS_DB . " SET clan='$last_ID', clan_invitations='-1,' WHERE id='$userID'";
            if (mysqli_query($link, $playerQuery)) {
                echo "done|" . $last_ID;
            } else {
                die(mysqli_error($link));
            }
        }
    } else {
        echo "Clan with this name already exist.";
    }
} else if ($type == 4) // get clan basic info
    {
    $query = "SELECT * FROM Clans WHERE id='$clanID'";
    $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_error($link));
    $numrow = mysqli_num_rows($result);
    if ($numrow > 0) {
        $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
        echo "yes" . "|" . $row['name'] . "|" . $row['members'];
        mysqli_free_result($result);
    } else {
        echo "Clan not exist.";
    }
} else if ($type == 5) //invite users
    {
    $query = "SELECT * FROM " . PLAYERS_DB . " WHERE nick='$userID'";
    $result = mysqli_query($link, $query) or die(mysqli_error($link));
    if ($result) {
        $numrow = mysqli_num_rows($result);
        if ($numrow > 0) {
            $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
            if ($row['clan'] == "-1") {
                $integerIDs = array_map('intval', explode(",", $row['clan_invitations']));
                if (in_array($clanID, $integerIDs) == false) {
                    if (count($integerIDs) < 10) {
                        $clanCon    = $clanID . ",";
                        $inserQuery = "UPDATE " . PLAYERS_DB . " SET clan_invitations=Concat('$clanCon',clan_invitations) WHERE nick='$userID'";
                        if (mysqli_query($link, $inserQuery) or die(mysqli_error($link))) {
                            echo "done";
                        }
                    } else {
                        echo "Player can't receive more invitations.";
                    }
                } else {
                    echo "Player already invited.";
                }
            } else {
                echo "This player is already in a Clan.";
            }
        } else {
            echo "Player with this nick not exist.";
        }
    }
} else if ($type == 6) //players info of joins requests
    {
    $idsArray = array_map('intval', preg_split('/,/', $userID, -1, PREG_SPLIT_NO_EMPTY));
    $array    = implode("','", $idsArray);
    if (count($idsArray) > 0) {
        $Findquery = "SELECT * FROM " . PLAYERS_DB . " WHERE id IN ('$array')";
        $findResult = mysqli_query($link, $Findquery) or die("Find Error: " . mysqli_error($link));
        $numrow2 = mysqli_num_rows($findResult);
        if ($numrow2 > 0) {
            echo "yes|";
            for ($i = 0; $i < $numrow2; $i++) {
                $user = mysqli_fetch_array($findResult);
                echo $user['nick'] . "," . $user['id'] . "|";
            }
        } else {
            echo "Can't find users: " . var_dump($idsArray);
        }
    } else {
        echo "No Joins Requests.";
    }
} else if ($type == 7) //accept join request (from clan managment)
    {
    $queryClan = "SELECT * FROM Clans WHERE id='$clanID'";
    $resultClan = mysqli_query($link, $queryClan) or die("Query 1: " . mysqli_error($link));
    $rowClan = mysqli_fetch_array($resultClan, MYSQLI_ASSOC);
    if ($rowClan['mcount'] < 20) {
        $query = "SELECT * FROM " . PLAYERS_DB . " WHERE id='$userID'";
        $result = mysqli_query($link, $query) or die("Query 2: " . mysqli_error($link));
        $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
        if ($row['clan'] == "-1") {
            $updateQuery = "UPDATE " . PLAYERS_DB . " SET clan='$clanID', clan_invitations='-1,' WHERE id='$userID'";
            $updateResult = mysqli_query($link, $updateQuery) or die("Query 3: " . mysqli_error($link));
            if ($updateResult) {
                $members         = $rowClan['members'] . $userID . "-0,";
                $userScore       = $row['score'];
                $updateClanQuery = "UPDATE Clans SET requests='$settings', mcount=mcount+1, members='$members', score=score+'$userScore' WHERE id='$clanID'";
                if (mysqli_query($link, $updateClanQuery) or die("Query 4: " . mysqli_error($link))) {
                    echo "done";
                }
            }
        } else {
            echo "Player already join in another Clan.";
        }
    } else {
        echo "Clan is full, can't accept more members.";
    }
} else if ($type == 8) //deny join request
    {
    $query = "UPDATE Clans SET requests='$userID' WHERE id='$clanID'";
    if (mysqli_query($link, $query) or die(mysqli_error($link))) {
        echo "done";
    }
} else if ($type == 9) //get clan info by clan name
    {
    $query = "SELECT * FROM Clans WHERE name='$clanID'";
    $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_error($link));
    $numrow = mysqli_num_rows($result);
    if ($numrow > 0) {
        $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
        echo "yes" . "|" . $row['id'] . "|" . $row['date'] . "|" . $row['members'] . "|" . $row['score'] . "|" . $row['description'] . "|" . $row['settings'] . "|";
        mysqli_free_result($result);
    } else {
        echo "Clan not exist.";
    }
} else if ($type == 10) //join to clan (from clan petition or public clan)
    {
    $query = "SELECT mcount,members FROM Clans WHERE id='$clanID'";
    $result = mysqli_query($link, $query) or die(mysqli_error($link));
    $row = mysqli_fetch_array($result);
    if ($row['mcount'] < 20) {
        $member         = $userID . "-0,";
        $newMembers     = $row['members'] . $member;
        $userRequestStr = $userID . ",";
        $query          = "UPDATE Clans, " . PLAYERS_DB . " SET Clans.members='$newMembers', Clans.mcount=Clans.mcount+1,Clans.score=Clans.score+'$settings',Clans.requests=REPLACE(Clans.requests,'$userRequestStr',''), " . PLAYERS_DB . ".clan='$clanID', " . PLAYERS_DB . ".clan_invitations='-1,' WHERE Clans.id='$clanID' AND " . PLAYERS_DB . ".id='$userID'";
        $updateResult = mysqli_query($link, $query) or die(mysqli_error($link));
        if ($updateResult) {
            echo "done";
        }
    } else {
        die("Clan is full.");
    }
} else if ($type == 11) //deny clan invitation
    {
    $query = "UPDATE " . PLAYERS_DB . " SET clan_invitations='$settings' WHERE id='$userID'";
    if (mysqli_query($link, $query) or die(mysqli_error($link))) {
        echo "done";
    }
} else if ($type == 12) //request clan to join
    {
    $query = "SELECT requests FROM Clans WHERE id='$clanID'";
    $result = mysqli_query($link, $query) or die(mysqli_error($link));
    $row = mysqli_fetch_array($result, MYSQLI_ASSOC);
    $ids = array_map('intval', explode(',', $row['requests']));
    if (!in_array($userID, $ids)) {
        if (count($ids) < 10) {
            $concatIds = $row['requests'] . $userID . ",";
            $query     = "UPDATE Clans SET requests='$concatIds' WHERE id='$clanID'";
            if (mysqli_query($link, $query) or die(mysqli_error($link))) {
                echo "done";
            }
        } else {
            echo "This clan can't receive more request at the moment.";
        }
    } else {
        echo "a request has already been sent.";
    }
} else if ($type == 13) //kick clan member
    {
    $userQuery = "SELECT score FROM " . PLAYERS_DB . " WHERE id='$userID'";
    $userResult = mysqli_query($link, $userQuery) or die(mysqli_error($link));
    $row         = mysqli_fetch_array($userResult, MYSQLI_ASSOC);
    $memberScore = $row['score'];
    $query       = "UPDATE Clans," . PLAYERS_DB . " SET Clans.members='$settings', Clans.mcount=Clans.mcount-1,Clans.score=Clans.score-'$memberScore', " . PLAYERS_DB . ".clan='-1' WHERE Clans.id='$clanID' AND " . PLAYERS_DB . ".id='$userID'";
    if (mysqli_query($link, $query) or die(mysqli_error($link))) {
        echo "yes";
    }
    if (isset($description) && $description == 1) {
        $deleteQuery = "DELETE FROM Clans WHERE id='$clanID'";
        mysqli_query($link, $deleteQuery) or die(mysqli_error($link));
    }
    mysqli_free_result($userResult);
} else if ($type == 14) //ascend or descent member
    {
    $query = "UPDATE Clans SET members='$settings' WHERE id='$clanID'";
    if (mysqli_query($link, $query) or die(mysqli_error($link))) {
        echo "yes";
    }
} else if ($type == 15) //get user invitations from clans
    {
    $idsArray = array_map('intval', preg_split('/,/', $clanID, -1, PREG_SPLIT_NO_EMPTY));
    $array    = implode("','", $idsArray);
    if (count($idsArray) > 0) {
        $query = "SELECT * FROM Clans WHERE id IN ('$array')";
        $findResult = mysqli_query($link, $query) or die("Find Error: " . mysqli_error($link));
        $numrow2 = mysqli_num_rows($findResult);
        if ($numrow2 > 0) {
            echo "yes|";
            for ($i = 0; $i < $numrow2; $i++) {
                $user = mysqli_fetch_array($findResult);
                echo $user['name'] . "," . $user['score'] . "," . $user['id'] . "|";
            }
        }
    } else {
        echo "No Invitations";
    }
} else if ($type == 16) //edit clan settings
    {
    $query = "UPDATE Clans SET settings='$settings', description='$description' WHERE id='$clanID'";
    if (mysqli_query($link, $query) or die(mysqli_error($link))) {
        echo "done";
    }
} else {
    echo "request not defined.";
}

mysqli_close($link);
?>