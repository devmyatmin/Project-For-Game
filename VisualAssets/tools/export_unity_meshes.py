#!/usr/bin/env python3
"""Export Unity YAML Mesh assets to Wavefront OBJ. Geometry, normals and UV0."""
from pathlib import Path
import re, struct, sys, yaml

ROOT = Path("VisualAssets/ExportedProject/Assets/Mesh")
OUT = Path("VisualAssets/OriginalModels/OBJ")
FMT_SIZE = {0:4, 1:2, 2:1, 3:1, 4:1, 5:1, 6:2, 7:2, 8:4, 9:4}
FMT_CODE = {0:"f", 1:"e", 2:"B", 3:"b", 4:"B", 5:"b", 6:"H", 7:"h", 8:"I", 9:"i"}

def load_mesh(path):
    raw = path.read_text(encoding="utf-8", errors="replace")
    raw = re.sub(r"^%YAML.*$|^%TAG.*$", "", raw, flags=re.M)
    raw = re.sub(r"^--- !u!\d+ &[-\d]+\s*$", "---", raw, flags=re.M)
    doc = yaml.safe_load(raw)
    if not isinstance(doc, dict) or "Mesh" not in doc:
        raise ValueError("Unity Mesh YAML root not found")
    return doc["Mesh"]

def export_one(path):
    mesh = load_mesh(path)
    vd = mesh.get("m_VertexData") or {}
    count = int(vd.get("m_VertexCount") or 0)
    channels = vd.get("m_Channels") or []
    blob = bytes.fromhex(vd.get("_typelessdata", ""))
    if count <= 0 or not channels or not blob:
        raise ValueError("vertex data missing")
    strides = {}
    for ch in channels:
        stream, fmt, dim, off = (int(ch.get(k,0)) for k in ("stream","format","dimension","offset"))
        if dim > 0 and fmt in FMT_SIZE:
            strides[stream] = max(strides.get(stream,0), off + dim*FMT_SIZE[fmt])
    stream_start, cursor = {}, 0
    for stream in sorted(strides):
        stream_start[stream] = cursor
        cursor += strides[stream] * count
    def read_channel(index):
        if index >= len(channels): return None
        ch = channels[index]
        stream, fmt, dim, off = (int(ch.get(k,0)) for k in ("stream","format","dimension","offset"))
        if dim <= 0 or stream not in strides or fmt not in FMT_CODE: return None
        st = struct.Struct("<" + FMT_CODE[fmt]*dim)
        return [st.unpack_from(blob, stream_start[stream]+off+i*strides[stream]) for i in range(count)]
    positions, normals, uv0 = read_channel(0), read_channel(1), read_channel(4)
    if not positions or any(len(p)<3 for p in positions): raise ValueError("position channel missing")
    ib = bytes.fromhex(mesh.get("m_IndexBuffer",""))
    isize = 2 if int(mesh.get("m_IndexFormat",0)) == 0 else 4
    if len(ib)%isize: raise ValueError("index buffer alignment error")
    ist = struct.Struct("<H" if isize==2 else "<I")
    indices = [ist.unpack_from(ib,i)[0] for i in range(0,len(ib),isize)]
    faces=[]
    for sub in mesh.get("m_SubMeshes") or []:
        if int(sub.get("topology",0)) != 0: continue
        begin=int(sub.get("firstByte",0))//isize
        subset=indices[begin:begin+int(sub.get("indexCount",0))]
        base=int(sub.get("baseVertex",0))
        for i in range(0,len(subset)-2,3):
            face=[subset[i+k]+base for k in range(3)]
            if all(0<=v<count for v in face): faces.append(face)
    if not faces: raise ValueError("no triangles found")
    normals = normals if normals and len(normals)==count and all(len(n)>=3 for n in normals) else None
    uv0 = uv0 if uv0 and len(uv0)==count and all(len(u)>=2 for u in uv0) else None
    OUT.mkdir(parents=True,exist_ok=True)
    target=OUT/(path.stem+".obj")
    with target.open("w",encoding="utf-8",newline="\n") as f:
        f.write("# Unity Mesh source: "+path.name+"\n")
        for p in positions: f.write("v "+" ".join(f"{float(x):.9g}" for x in p[:3])+"\n")
        if normals:
            for n in normals: f.write("vn "+" ".join(f"{float(x):.9g}" for x in n[:3])+"\n")
        if uv0:
            for u in uv0: f.write(f"vt {float(u[0]):.9g} {float(u[1]):.9g}\n")
        for face in faces:
            vals=[]
            for v0 in face:
                v=v0+1
                vals.append(f"{v}/{v}/{v}" if uv0 and normals else f"{v}/{v}" if uv0 else f"{v}//{v}" if normals else str(v))
            f.write("f "+" ".join(vals)+"\n")
    return len(positions),len(faces),target

def main():
    if not ROOT.exists():
        print("Missing mesh folder: "+str(ROOT),file=sys.stderr); return 2
    files=sorted(ROOT.glob("*.asset")); ok=0; errors=[]; vtotal=ftotal=0
    for path in files:
        try:
            v,f,out=export_one(path); ok+=1; vtotal+=v; ftotal+=f
            print(f"OK {path.name}: {v} verts, {f} triangles => {out}")
        except Exception as exc:
            errors.append((path.name,str(exc))); print(f"SKIP {path.name}: {exc}",file=sys.stderr)
    report=OUT.parent/"EXPORT_REPORT.md"; OUT.parent.mkdir(parents=True,exist_ok=True)
    details="".join("- "+name+": "+reason+"\n" for name,reason in errors)
    report.write_text("# Unity Mesh OBJ export report\n\n"
      f"- Source assets: {len(files)}\n- Exported: {ok}\n- Skipped: {len(errors)}\n"
      f"- Vertices: {vtotal}\n- Triangles: {ftotal}\n\n"
      "OBJ includes geometry and available vertex normals/UV0. Unity materials, shader data, "
      "rigs, blend shapes, animations, and prefab/scene hierarchy are not reconstructed.\n\n"
      + ("## Skipped assets\n\n"+details if errors else "All source meshes exported.\n"),encoding="utf-8")
    print(f"Result {ok}/{len(files)} successful.")
    return 0 if ok else 1
if __name__=="__main__": raise SystemExit(main())
