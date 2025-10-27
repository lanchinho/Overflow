import z from "zod";

const required = (propName: string) => z.string().trim().min(1, {message: `${propName} is required`});

export const editProfileSchema = z.object({
  displayName: required("Display name"),
  description: required ("Description")    
});

export type EditProfileSchema = z.infer<typeof editProfileSchema>;