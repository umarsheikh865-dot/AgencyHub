import { useEffect, useRef, useState } from "react";
import { FaceDetector, FilesetResolver } from "@mediapipe/tasks-vision";

const WASM_PATH =
    "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision/wasm";

const MODEL_PATH =
    "https://storage.googleapis.com/mediapipe-models/face_detector/blaze_face_short_range/float16/1/blaze_face_short_range.tflite";

function FaceVerification() {
    const videoRef = useRef(null);
    const streamRef = useRef(null);
    const detectorRef = useRef(null);
    const animationRef = useRef(null);

    const [cameraStarted, setCameraStarted] = useState(false);
    const [faceDetected, setFaceDetected] = useState(false);
    const [status, setStatus] = useState("Camera is not started.");
    const [error, setError] = useState("");

    useEffect(() => {
        return () => {
            stopCamera();
        };
    }, []);

    const createDetector = async () => {
        if (detectorRef.current) {
            return detectorRef.current;
        }

        const vision = await FilesetResolver.forVisionTasks(
            WASM_PATH
        );

        const detector = await FaceDetector.createFromOptions(
            vision,
            {
                baseOptions: {
                    modelAssetPath: MODEL_PATH
                },

                runningMode: "VIDEO",

                minDetectionConfidence: 0.5
            }
        );

        detectorRef.current = detector;

        return detector;
    };

    const startCamera = async () => {
        try {
            setError("");
            setStatus("Requesting camera permission...");

            const stream =
                await navigator.mediaDevices.getUserMedia({
                    video: {
                        width: {
                            ideal: 1280
                        },
                        height: {
                            ideal: 720
                        },
                        facingMode: "user"
                    },

                    audio: false
                });

            streamRef.current = stream;

            if (!videoRef.current) {
                return;
            }

            videoRef.current.srcObject = stream;

            await videoRef.current.play();

            await createDetector();

            setCameraStarted(true);
            setStatus("Camera ready. Looking for a face...");

            detectFace();
        } catch (err) {
            console.error(err);

            setError(
                "Unable to access the camera. Please allow camera permission and try again."
            );

            setStatus("Camera unavailable.");
        }
    };

    const detectFace = () => {
        if (!videoRef.current ||
            !detectorRef.current) {
            return;
        }

        if (
            videoRef.current.readyState <
            HTMLMediaElement.HAVE_CURRENT_DATA
        ) {
            animationRef.current =
                requestAnimationFrame(detectFace);

            return;
        }

        try {
            const result =
                detectorRef.current.detectForVideo(
                    videoRef.current,
                    performance.now()
                );

            const detected =
                result?.detections?.length > 0;

            setFaceDetected(detected);

            if (detected) {
                setStatus("Face detected.");
            } else {
                setStatus("Looking for a face...");
            }
        } catch (err) {
            console.error(err);
        }

        animationRef.current =
            requestAnimationFrame(detectFace);
    };

    const stopCamera = () => {
        if (animationRef.current) {
            cancelAnimationFrame(
                animationRef.current
            );

            animationRef.current = null;
        }

        if (streamRef.current) {
            streamRef.current
                .getTracks()
                .forEach((track) => track.stop());

            streamRef.current = null;
        }

        if (videoRef.current) {
            videoRef.current.srcObject = null;
        }

        setCameraStarted(false);
        setFaceDetected(false);
        setStatus("Camera is not started.");
    };

    return (
        <div style={styles.page}>
            <div style={styles.header}>
                <div>
                    <h1 style={styles.title}>
                        Face Verification
                    </h1>

                    <p style={styles.subtitle}>
                        Secure camera-based face detection
                    </p>
                </div>

                <div style={styles.securityBadge}>
                    🔐 Secure
                </div>
            </div>

            <div style={styles.grid}>
                <div style={styles.cameraCard}>
                    <div style={styles.cardHeader}>
                        <div>
                            <h2 style={styles.cardTitle}>
                                Identity Verification
                            </h2>

                            <p style={styles.cardSubtitle}>
                                Position your face inside the camera
                                view.
                            </p>
                        </div>

                        <div
                            style={{
                                ...styles.statusBadge,
                                background:
                                    faceDetected
                                        ? "#dcfce7"
                                        : "#f3f4f6",
                                color:
                                    faceDetected
                                        ? "#166534"
                                        : "#4b5563"
                            }}
                        >
                            <span
                                style={{
                                    ...styles.statusDot,
                                    background:
                                        faceDetected
                                            ? "#22c55e"
                                            : "#9ca3af"
                                }}
                            />

                            {faceDetected
                                ? "Face Detected"
                                : "Waiting"}
                        </div>
                    </div>

                    <div style={styles.cameraWrapper}>
                        <video
                            ref={videoRef}
                            autoPlay
                            muted
                            playsInline
                            style={styles.video}
                        />

                        {!cameraStarted && (
                            <div style={styles.cameraOverlay}>
                                <div style={styles.cameraIcon}>
                                    📷
                                </div>

                                <strong>
                                    Camera not started
                                </strong>

                                <span>
                                    Click Start Camera below
                                </span>
                            </div>
                        )}

                        {cameraStarted && (
                            <div
                                style={{
                                    ...styles.faceFrame,
                                    borderColor:
                                        faceDetected
                                            ? "#22c55e"
                                            : "#6366f1"
                                }}
                            />
                        )}
                    </div>

                    <div style={styles.controls}>
                        {!cameraStarted ? (
                            <button
                                type="button"
                                onClick={startCamera}
                                style={styles.primaryButton}
                            >
                                📷 Start Camera
                            </button>
                        ) : (
                            <button
                                type="button"
                                onClick={stopCamera}
                                style={styles.dangerButton}
                            >
                                Stop Camera
                            </button>
                        )}
                    </div>

                    {error && (
                        <div style={styles.error}>
                            {error}
                        </div>
                    )}
                </div>

                <div style={styles.infoCard}>
                    <h2 style={styles.cardTitle}>
                        Verification Status
                    </h2>

                    <div style={styles.statusPanel}>
                        <div style={styles.checkIcon}>
                            {cameraStarted
                                ? "✓"
                                : "○"}
                        </div>

                        <div>
                            <strong>
                                Camera
                            </strong>

                            <p>
                                {cameraStarted
                                    ? "Connected"
                                    : "Not connected"}
                            </p>
                        </div>
                    </div>

                    <div style={styles.statusPanel}>
                        <div style={styles.checkIcon}>
                            {faceDetected
                                ? "✓"
                                : "○"}
                        </div>

                        <div>
                            <strong>
                                Face Detection
                            </strong>

                            <p>
                                {faceDetected
                                    ? "Face detected successfully"
                                    : "Waiting for face"}
                            </p>
                        </div>
                    </div>

                    <div
                        style={{
                            ...styles.resultBox,
                            background:
                                faceDetected
                                    ? "#f0fdf4"
                                    : "#f8fafc"
                        }}
                    >
                        <div style={styles.resultIcon}>
                            {faceDetected
                                ? "✓"
                                : "👤"}
                        </div>

                        <strong>
                            {faceDetected
                                ? "Face detected"
                                : "Ready for verification"}
                        </strong>

                        <p>
                            {faceDetected
                                ? "The camera has detected a face in the video stream."
                                : "Start your camera and position your face in front of it."}
                        </p>
                    </div>

                    <div style={styles.privacyBox}>
                        <strong>
                            🔒 Privacy
                        </strong>

                        <p>
                            This demo performs face detection in the
                            browser and does not upload or store your
                            camera image.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}

const styles = {
    page: {
        minHeight: "calc(100vh - 70px)",
        background: "#f6f8fb",
        padding: "28px"
    },

    header: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        marginBottom: "24px"
    },

    title: {
        margin: 0,
        color: "#111827",
        fontSize: "30px"
    },

    subtitle: {
        margin: "6px 0 0",
        color: "#6b7280"
    },

    securityBadge: {
        padding: "9px 14px",
        borderRadius: "20px",
        background: "#ecfdf5",
        color: "#166534",
        fontWeight: "700",
        fontSize: "13px"
    },

    grid: {
        display: "grid",
        gridTemplateColumns:
            "minmax(0, 1fr) 350px",
        gap: "22px",
        maxWidth: "1350px"
    },

    cameraCard: {
        background: "white",
        border: "1px solid #e5e7eb",
        borderRadius: "16px",
        padding: "22px",
        boxShadow:
            "0 8px 30px rgba(15, 23, 42, 0.05)"
    },

    infoCard: {
        background: "white",
        border: "1px solid #e5e7eb",
        borderRadius: "16px",
        padding: "22px",
        height: "fit-content",
        boxShadow:
            "0 8px 30px rgba(15, 23, 42, 0.05)"
    },

    cardHeader: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "flex-start",
        gap: "15px",
        marginBottom: "20px"
    },

    cardTitle: {
        margin: 0,
        fontSize: "19px",
        color: "#111827"
    },

    cardSubtitle: {
        margin: "6px 0 0",
        color: "#6b7280",
        fontSize: "13px"
    },

    statusBadge: {
        padding: "7px 10px",
        borderRadius: "20px",
        fontSize: "12px",
        fontWeight: "700",
        whiteSpace: "nowrap"
    },

    statusDot: {
        display: "inline-block",
        width: "7px",
        height: "7px",
        borderRadius: "50%",
        marginRight: "6px"
    },

    cameraWrapper: {
        position: "relative",
        width: "100%",
        minHeight: "430px",
        background: "#111827",
        borderRadius: "14px",
        overflow: "hidden"
    },

    video: {
        width: "100%",
        height: "430px",
        objectFit: "cover",
        display: "block",
        transform: "scaleX(-1)"
    },

    cameraOverlay: {
        position: "absolute",
        inset: 0,
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center",
        color: "white",
        gap: "8px"
    },

    cameraIcon: {
        fontSize: "42px",
        marginBottom: "10px"
    },

    faceFrame: {
        position: "absolute",
        left: "50%",
        top: "50%",
        width: "230px",
        height: "290px",
        transform: "translate(-50%, -50%)",
        border: "3px solid",
        borderRadius: "45%"
    },

    controls: {
        display: "flex",
        justifyContent: "center",
        paddingTop: "18px"
    },

    primaryButton: {
        border: "none",
        background: "#111827",
        color: "white",
        padding: "12px 22px",
        borderRadius: "9px",
        cursor: "pointer",
        fontWeight: "700"
    },

    dangerButton: {
        border: "none",
        background: "#dc2626",
        color: "white",
        padding: "12px 22px",
        borderRadius: "9px",
        cursor: "pointer",
        fontWeight: "700"
    },

    error: {
        marginTop: "15px",
        padding: "12px",
        borderRadius: "9px",
        background: "#fef2f2",
        color: "#b91c1c",
        fontSize: "13px"
    },

    statusPanel: {
        display: "flex",
        alignItems: "center",
        gap: "12px",
        padding: "14px 0",
        borderBottom: "1px solid #f0f0f0"
    },

    checkIcon: {
        width: "34px",
        height: "34px",
        borderRadius: "50%",
        background: "#eef2ff",
        color: "#4f46e5",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        fontWeight: "800"
    },

    statusPanelStrong: {
        color: "#111827"
    },

    resultBox: {
        marginTop: "22px",
        padding: "18px",
        borderRadius: "12px",
        textAlign: "center"
    },

    resultIcon: {
        fontSize: "30px",
        marginBottom: "8px"
    },

    resultBoxStrong: {
        color: "#111827"
    },

    resultBoxP: {
        color: "#6b7280",
        fontSize: "12px",
        lineHeight: "1.5"
    },

    privacyBox: {
        marginTop: "18px",
        padding: "14px",
        borderRadius: "10px",
        background: "#eff6ff",
        border: "1px solid #dbeafe"
    }
};

export default FaceVerification;