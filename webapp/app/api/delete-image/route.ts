import { cloudinary } from "@/lib/cloudinary";

export async function POST(request : Request){
  const publicId = await request.json(); 

  try{
    await cloudinary.v2.uploader.destroy(publicId);
    return new Response(null, {status: 200});
  } catch (error) {
    console.log("Cloudinary delete failed");
    return new Response("Failed to delete" + error, {status: 500});
  }
}