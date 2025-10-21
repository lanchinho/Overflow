"use client";

import { testAuth } from "@/lib/actions/auth-actions";
import { handleError, successToast } from "@/lib/util";
import { Button } from "@heroui/button";

const onClick = async () =>{
  const {data, error} = await testAuth();  
  if(error) handleError(error);
  if(data) successToast(data);
};

export default function AuthTestButton() {
  return (
    <Button
      color="success"
      onPress={onClick}
    >
        Test Auth
    </Button>
  );
}
