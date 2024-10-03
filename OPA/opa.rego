package app.abac

import rego.v1

default allow := false

allow if {
    user_is_customer
    user_is_adult
    input.DrinkName == "beer"
    }
    
allow if {
    user_is_customer
    input.DrinkName == "fristi"
}

user_is_bartender if data.user_attributes[input.user].role[_] == "bartender"

user_is_customer if data.user_attributes[input.user].role[_] == "customer"

user_is_adult if data.user_attributes[input.user].age >= 18

user_is_underage if data.user_attributes[input.user].age < 18