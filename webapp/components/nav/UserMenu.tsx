"use client";

import { Dropdown, DropdownItem, DropdownMenu, DropdownTrigger } from "@heroui/dropdown";
import { Avatar } from "@heroui/react";
import { User } from "next-auth";
import { signOut } from "next-auth/react";

type Props ={
    user: User;
}

export default function UserMenu({user} : Props) {
  return (
    <Dropdown>
      <DropdownTrigger>
        <div className="flex items-center gap-2 cursor-pointer">
          <Avatar color='secondary' size='sm' name={user.name?.charAt(0).toUpperCase()}/>
        </div>            
      </DropdownTrigger>
      <DropdownMenu>
        <DropdownItem key='edit'>Edit Profile</DropdownItem>
        <DropdownItem
          key='logot'
          className="text-danger"
          color ='danger'
          onClick={()=>  signOut({redirectTo: "/"})}
        >Sign out</DropdownItem>
      </DropdownMenu>
    </Dropdown>
  );
}
