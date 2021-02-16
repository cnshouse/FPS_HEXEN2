<?php
include_once("bl_Common.php");

Class Functions
{
private $conn;

public function __construct($connection) {
  $this->conn = $connection;
}

/*
*
* Update user single data in the User table.
*
*/
public function update_user_row($field, $value, $where, $identifier)
{
  $sql = "UPDATE " . PLAYERS_DB. " SET " . $field . "='" . $value . "' WHERE " . $where . "='" . $identifier . "'";
  $this->Query($sql);
   
  return mysqli_affected_rows($this->conn) > 0;
}

/*
*
* Update user data in the User table.
*
*/
public function update_user_data($fields, $values, $where, $identifier)
{
  $inserts = $this->string_to_assoc_array($fields,$values);

  if(!is_array($inserts))
  {
     die("Invalid format, couldn't parse fiels and values.");
  }

  $sql = "UPDATE " . PLAYERS_DB. " SET ";
  foreach(array_keys($inserts) as $key)
  {
    $sql .= $key  . "='" . $inserts[$key] . "', ";
  }
  $sql = rtrim($sql, ", ");
  $sql .= " WHERE " . $where . "='". $identifier . "'";
  $this->Query($sql);
   
  return mysqli_affected_rows($this->conn) > 0;
}

/*
*
* Get an user by the given identifier.
*
*/
public function get_user_by($field, $value)
{
  $result = $this->Query("SELECT * FROM " . PLAYERS_DB . " WHERE ".$field."= '$value'");

  if(mysqli_num_rows($result) <= 0)
  {
    mysqli_free_result($result);
      return false;
  }

  return $result;
}

/*
*
* Get a single row from a user.
*
*/
public function get_user_row($user_id, $row_name)
{
  $sql = "SELECT ". $row_name . " FROM " . PLAYERS_DB . " WHERE id='" . $user_id . "'";
  $result = $this->Query($sql);
  $row = mysqli_fetch_assoc($result);
  return $row[$row_name];
}

/*
*
* Check if an user exist
*
*/
public function user_exist($user_id)
{
  $sql = "SELECT * FROM " . PLAYERS_DB . " WHERE id='" . $user_id . "'";
  $result = $this->Query($sql);
  return mysqli_num_rows($result) > 0;
}

/*
*
* Check if an user exist
*
*/
public function user_exist_custom($where, $index)
{
  $sql = "SELECT * FROM " . PLAYERS_DB . " WHERE $where='" . $index . "'";
  $result = $this->Query($sql);
  return mysqli_num_rows($result) > 0;
}

/*
*
* Execute a mysqli query and exit on error.
*
*/
public function Query($query)
{
    $result = mysqli_query($this->conn, $query);
    if(mysqli_error($this->conn))
    {
      die(mysqli_error($this->conn));
    }
    return $result;
}

/*
*
* Execute multiple mysqli query and exit on error.
*
*/
public function multiple_query($query)
{
    $finalResult = mysqli_multi_query($this->conn, $query);
    if ($finalResult) {
      do {
          if ($result = mysqli_store_result($this->conn) && mysqli_error($this->conn) == '') {
              mysqli_free_result($result);
          }else{

          }
      } while (mysqli_more_results($this->conn) && mysqli_next_result($this->conn));
  }else{
    die('Multi Query Fail: ' . mysqli_error($this->conn));
  }
    return $finalResult;
}

/*
*
* Convert strings of fields and values
* to associative array.
*/
public function string_to_assoc_array($fields,$values)
{
  $array_fields = $fields;
  $array_values = $values;
  if(!is_array($fields))
  {
    $array_fields = explode('|', $fields);
    $array_values = explode('|', $values);
  } 

  return array_combine($array_fields,$array_values);
}

}
?>